using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Api.Authorization;
using ChargingControlSystem.Data.Enums;
using ChargingControlSystem.Api.Services;
using Swashbuckle.AspNetCore.Annotations;
using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/user-portal")]
[RequireRole(UserRole.User, UserRole.TenantAdmin, UserRole.SuperAdmin)]
public class UserPortalController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITariffService _tariffService;
    private readonly ILogger<UserPortalController> _logger;

    public UserPortalController(
        ApplicationDbContext context,
        ITariffService tariffService,
        ILogger<UserPortalController> logger)
    {
        _context = context;
        _tariffService = tariffService;
        _logger = logger;
    }

    // GET /api/user-portal/dashboard
    // Dashboard-Statistiken für den eingeloggten User
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Total sessions
        var totalSessions = await _context.ChargingSessions
            .Where(s => s.UserId == userId)
            .CountAsync();

        // Active session
        var activeSession = await _context.ChargingSessions
            .Where(s => s.UserId == userId && s.EndedAt == null)
            .CountAsync();

        // Total energy consumed (kWh)
        var totalEnergy = await _context.ChargingSessions
            .Where(s => s.UserId == userId)
            .SumAsync(s => (decimal?)s.EnergyDelivered) ?? 0;

        // Total costs
        var totalCosts = await _context.ChargingSessions
            .Where(s => s.UserId == userId)
            .SumAsync(s => (decimal?)s.Cost) ?? 0;

        // Sessions this month
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var sessionsThisMonth = await _context.ChargingSessions
            .Where(s => s.UserId == userId && s.StartedAt >= startOfMonth)
            .CountAsync();

        // Costs this month
        var costsThisMonth = await _context.ChargingSessions
            .Where(s => s.UserId == userId && s.StartedAt >= startOfMonth)
            .SumAsync(s => (decimal?)s.Cost) ?? 0;

        // Available stations count (based on user group memberships)
        var availableStationsCount = await GetAvailableStationsCount(userId);

        return Ok(new
        {
            TotalSessions = totalSessions,
            ActiveSession = activeSession,
            TotalEnergy = totalEnergy,
            TotalCosts = totalCosts,
            SessionsThisMonth = sessionsThisMonth,
            CostsThisMonth = costsThisMonth,
            AvailableStations = availableStationsCount
        });
    }

    // GET /api/user-portal/available-stations
    // Alle Ladestationen, zu denen der User Zugriff hat (basierend auf Gruppenmitgliedschaften + private Stationen)
    [HttpGet("available-stations")]
    [SwaggerOperation(
        Summary = "Verfügbare Ladestationen abrufen",
        Description = "Ruft alle Ladestationen ab, zu denen der Benutzer Zugriff hat (Gruppenzugriff + eigene private Stationen)."
    )]
    public async Task<IActionResult> GetAvailableStations()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Get all charging station groups the user has access to via their user groups
        var userGroupIds = await _context.UserGroupMemberships
            .Where(m => m.UserId == userId)
            .Select(m => m.UserGroupId)
            .ToListAsync();

        var chargingStationGroupIds = await _context.UserGroupChargingStationGroupPermissions
            .Where(p => userGroupIds.Contains(p.UserGroupId))
            .Select(p => p.ChargingStationGroupId)
            .Distinct()
            .ToListAsync();

        // Get all stations in these groups
        var stationIds = await _context.ChargingStationGroupMemberships
            .Where(m => chargingStationGroupIds.Contains(m.ChargingStationGroupId))
            .Select(m => m.ChargingStationId)
            .Distinct()
            .ToListAsync();

        // Get public stations the user has access to
        var publicStations = await _context.ChargingStations
            .Include(s => s.ChargingPark)
                .ThenInclude(p => p.Tenant)
            .Where(s => stationIds.Contains(s.Id) && s.IsActive && !s.IsPrivate)
            .Select(s => new
            {
                s.Id,
                s.StationId,
                s.Name,
                Status = s.Status.ToString(),
                s.Vendor,
                s.Model,
                Type = s.Type.ToString(),
                s.MaxPower,
                s.NumberOfConnectors,
                s.Latitude,
                s.Longitude,
                IsPrivate = false,
                ChargingPark = new
                {
                    s.ChargingPark.Id,
                    s.ChargingPark.Name,
                    s.ChargingPark.Address,
                    s.ChargingPark.City,
                    Tenant = new
                    {
                        s.ChargingPark.Tenant.Id,
                        s.ChargingPark.Tenant.Name
                    }
                },
                Groups = _context.ChargingStationGroupMemberships
                    .Where(m => m.ChargingStationId == s.Id)
                    .Select(m => new
                    {
                        m.ChargingStationGroup.Id,
                        m.ChargingStationGroup.Name
                    })
                    .ToList(),
                s.LastHeartbeat
            })
            .ToListAsync();

        // Get user's private stations
        var privateStations = await _context.ChargingStations
            .Where(s => s.IsPrivate && s.OwnerId == userId && s.IsActive)
            .Select(s => new
            {
                s.Id,
                s.StationId,
                s.Name,
                Status = s.Status.ToString(),
                s.Vendor,
                s.Model,
                Type = s.Type.ToString(),
                s.MaxPower,
                s.NumberOfConnectors,
                s.Latitude,
                s.Longitude,
                IsPrivate = true,
                ChargingPark = (object?)null,
                Groups = new List<object>(),
                s.LastHeartbeat
            })
            .ToListAsync();

        // Combine both lists
        var allStations = publicStations.Concat<object>(privateStations).ToList();

        return Ok(allStations);
    }

    // GET /api/user-portal/debug-access
    // Debug-Endpunkt um zu sehen, welche Berechtigungen ein User hat
    [HttpGet("debug-access")]
    public async Task<IActionResult> GetDebugAccess()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var userGroups = await _context.UserGroupMemberships
            .Include(m => m.UserGroup)
            .Where(m => m.UserId == userId)
            .Select(m => new
            {
                UserGroupId = m.UserGroupId,
                UserGroupName = m.UserGroup.Name,
                TenantId = m.UserGroup.TenantId
            })
            .ToListAsync();

        var userGroupIds = userGroups.Select(ug => ug.UserGroupId).ToList();

        var permissions = await _context.UserGroupChargingStationGroupPermissions
            .Include(p => p.ChargingStationGroup)
            .Where(p => userGroupIds.Contains(p.UserGroupId))
            .Select(p => new
            {
                UserGroupId = p.UserGroupId,
                ChargingStationGroupId = p.ChargingStationGroupId,
                ChargingStationGroupName = p.ChargingStationGroup.Name
            })
            .ToListAsync();

        var chargingStationGroupIds = permissions.Select(p => p.ChargingStationGroupId).Distinct().ToList();

        var stationMemberships = await _context.ChargingStationGroupMemberships
            .Include(m => m.ChargingStation)
            .Where(m => chargingStationGroupIds.Contains(m.ChargingStationGroupId))
            .Select(m => new
            {
                ChargingStationGroupId = m.ChargingStationGroupId,
                ChargingStationId = m.ChargingStationId,
                ChargingStationName = m.ChargingStation.Name
            })
            .ToListAsync();

        return Ok(new
        {
            UserId = userId,
            UserGroups = userGroups,
            Permissions = permissions,
            StationMemberships = stationMemberships,
            Summary = new
            {
                UserGroupCount = userGroups.Count,
                PermissionCount = permissions.Count,
                AccessibleStationCount = stationMemberships.Select(m => m.ChargingStationId).Distinct().Count()
            }
        });
    }

    // GET /api/user-portal/charging-sessions
    // Eigene Ladevorgänge mit Details
    [HttpGet("charging-sessions")]
    public async Task<IActionResult> GetChargingSessions([FromQuery] int? limit = 50)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var query = _context.ChargingSessions
            .Include(s => s.Vehicle)
            .Include(s => s.ChargingConnector)
                .ThenInclude(c => c.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
                        .ThenInclude(st => st.ChargingPark)
            .Include(s => s.AuthorizationMethod)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt);

        var sessions = await (limit.HasValue ? query.Take(limit.Value) : query)
            .Select(s => new
            {
                s.Id,
                s.StartedAt,
                s.EndedAt,
                Duration = s.EndedAt.HasValue 
                    ? (int)(s.EndedAt.Value - s.StartedAt).TotalMinutes 
                    : (int)(DateTime.UtcNow - s.StartedAt).TotalMinutes,
                s.EnergyDelivered,
                s.Cost,
                Status = s.Status.ToString(),
                Vehicle = s.Vehicle != null ? new
                {
                    s.Vehicle.Id,
                    Make = s.Vehicle.Make,
                    Model = s.Vehicle.Model,
                    LicensePlate = s.Vehicle.LicensePlate
                } : null,
                Station = new
                {
                    s.ChargingConnector.ChargingPoint.ChargingStation.Id,
                    s.ChargingConnector.ChargingPoint.ChargingStation.Name,
                    s.ChargingConnector.ChargingPoint.ChargingStation.StationId,
                    ChargingPark = new
                    {
                        s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Name,
                        s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Address,
                        s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.City
                    }
                },
                Connector = new
                {
                    s.ChargingConnector.Id,
                    s.ChargingConnector.ConnectorId,
                    Type = s.ChargingConnector.ConnectorType
                },
                AuthorizationMethod = s.AuthorizationMethod != null ? new
                {
                    s.AuthorizationMethod.Id,
                    Type = s.AuthorizationMethod.Type.ToString(),
                    s.AuthorizationMethod.FriendlyName,
                    s.AuthorizationMethod.Identifier
                } : null
            })
            .ToListAsync();

        return Ok(sessions);
    }

    // GET /api/user-portal/costs
    // Kostenübersicht mit Breakdown
    [HttpGet("costs")]
    public async Task<IActionResult> GetCosts([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var currentYear = year ?? DateTime.UtcNow.Year;
        var currentMonth = month ?? DateTime.UtcNow.Month;

        // Monthly costs for the current year
        var monthlyCosts = await _context.ChargingSessions
            .Where(s => s.UserId == userId && s.StartedAt.Year == currentYear)
            .GroupBy(s => s.StartedAt.Month)
            .Select(g => new
            {
                Month = g.Key,
                TotalCost = g.Sum(s => (decimal?)s.Cost) ?? 0,
                TotalEnergy = g.Sum(s => (decimal?)s.EnergyDelivered) ?? 0,
                SessionCount = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        // Total costs per charging park (lifetime)
        var costsByPark = await _context.ChargingSessions
            .Include(s => s.ChargingConnector)
                .ThenInclude(c => c.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
                        .ThenInclude(st => st.ChargingPark)
            .Where(s => s.UserId == userId)
            .GroupBy(s => new
            {
                ParkId = s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Id,
                ParkName = s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Name
            })
            .Select(g => new
            {
                g.Key.ParkId,
                g.Key.ParkName,
                TotalCost = g.Sum(s => (decimal?)s.Cost) ?? 0,
                SessionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalCost)
            .ToListAsync();

        return Ok(new
        {
            Year = currentYear,
            Month = currentMonth,
            MonthlyCosts = monthlyCosts,
            CostsByPark = costsByPark
        });
    }

    // GET /api/user-portal/billing-transactions
    // Eigene Billing-Transaktionen
    [HttpGet("billing-transactions")]
    public async Task<IActionResult> GetBillingTransactions()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var transactions = await _context.BillingTransactions
            .Include(t => t.ChargingSession)
                .ThenInclude(s => s!.ChargingConnector)
                    .ThenInclude(c => c.ChargingPoint)
                        .ThenInclude(cp => cp.ChargingStation)
            .Where(t => t.ChargingSession!.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Amount,
                t.Currency,
                t.Description,
                Status = t.Status.ToString(),
                t.CreatedAt,
                t.ProcessedAt,
                Session = t.ChargingSession != null ? new
                {
                    t.ChargingSession.Id,
                    t.ChargingSession.EnergyDelivered,
                    t.ChargingSession.StartedAt,
                    t.ChargingSession.EndedAt,
                    Station = t.ChargingSession.ChargingConnector != null
                        ? t.ChargingSession.ChargingConnector.ChargingPoint.ChargingStation.Name
                        : "Unknown"
                } : null
            })
            .ToListAsync();

        return Ok(transactions);
    }

    // GET /api/user-portal/charging-sessions/{id}/cost-breakdown
    // Detaillierte Kostenaufschlüsselung für eine Session
    [HttpGet("charging-sessions/{id}/cost-breakdown")]
    [SwaggerOperation(
        Summary = "Kostenaufschlüsselung für eine Session",
        Description = "Ruft die detaillierte Kostenaufschlüsselung für einen Ladevorgang ab, inklusive Tarif-Details und Aufschlüsselung nach Komponenten."
    )]
    [SwaggerResponse(200, "Kostenaufschlüsselung erfolgreich abgerufen")]
    [SwaggerResponse(404, "Session nicht gefunden")]
    [SwaggerResponse(403, "Keine Berechtigung für diese Session")]
    public async Task<IActionResult> GetSessionCostBreakdown(Guid id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Load session with all related data
        var session = await _context.ChargingSessions
            .Include(s => s.Vehicle)
            .Include(s => s.ChargingConnector)
                .ThenInclude(c => c.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
                        .ThenInclude(st => st.ChargingPark)
            .Include(s => s.AuthorizationMethod)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return NotFound(new { message = "Session nicht gefunden" });

        // Check if user has access to this session
        if (session.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access session {SessionId} belonging to user {SessionUserId}", 
                userId, id, session.UserId);
            return Forbid();
        }

        var sessionEnd = session.EndedAt ?? DateTime.UtcNow;
        var durationMinutes = (int)(sessionEnd - session.StartedAt).TotalMinutes;

        // Calculate cost breakdown using tariff service
        TariffCalculationResult? costCalculation = null;
        if (session.UserId.HasValue && session.EndedAt.HasValue)
        {
            try
            {
                costCalculation = await _tariffService.CalculateCostAsync(
                    session.UserId.Value,
                    session.ChargingConnector.ChargingPoint.ChargingStationId,
                    session.StartedAt,
                    session.EndedAt.Value,
                    session.EnergyDelivered
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cost breakdown for session {SessionId}", id);
            }
        }

        // Build response
        var response = new
        {
            SessionId = session.Id,
            SessionNumber = session.SessionId,
            OcppTransactionId = session.OcppTransactionId,
            
            // Time information
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            DurationMinutes = durationMinutes,
            DurationFormatted = $"{durationMinutes / 60}h {durationMinutes % 60}m",
            
            // Energy information
            EnergyDelivered = session.EnergyDelivered,
            EnergyDeliveredFormatted = $"{session.EnergyDelivered:F2} kWh",
            
            // Cost information
            TotalCost = session.Cost,
            Currency = costCalculation?.Currency ?? "EUR",
            TotalCostFormatted = $"{session.Cost:F2} {(costCalculation?.Currency ?? "EUR")}",
            
            // Cost breakdown
            CostBreakdown = costCalculation != null 
                ? costCalculation.Breakdown.Select(kvp => new
                {
                    Component = kvp.Key,
                    Cost = kvp.Value,
                    CostFormatted = $"{kvp.Value:F2} {costCalculation.Currency}"
                }).ToList() as object
                : new List<object>(),
            
            // Tariff information
            AppliedTariff = costCalculation?.AppliedTariff != null ? new
            {
                costCalculation.AppliedTariff.Id,
                costCalculation.AppliedTariff.Name,
                costCalculation.AppliedTariff.Description,
                costCalculation.AppliedTariff.Currency
            } : null,
            
            // Station information
            Station = new
            {
                session.ChargingConnector.ChargingPoint.ChargingStation.Id,
                session.ChargingConnector.ChargingPoint.ChargingStation.Name,
                session.ChargingConnector.ChargingPoint.ChargingStation.StationId,
                ChargingPark = new
                {
                    session.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Name,
                    session.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Address,
                    session.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.City,
                    session.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.PostalCode
                }
            },
            
            // Connector information
            Connector = new
            {
                session.ChargingConnector.Id,
                session.ChargingConnector.ConnectorId,
                Type = session.ChargingConnector.ConnectorType
            },
            
            // Vehicle information
            Vehicle = session.Vehicle != null ? new
            {
                session.Vehicle.Id,
                session.Vehicle.Make,
                session.Vehicle.Model,
                session.Vehicle.LicensePlate
            } : null,
            
            // Authorization information
            AuthorizationMethod = session.AuthorizationMethod != null ? new
            {
                session.AuthorizationMethod.Id,
                Type = session.AuthorizationMethod.Type.ToString(),
                session.AuthorizationMethod.FriendlyName,
                session.AuthorizationMethod.Identifier
            } : null,
            
            // Status
            Status = session.Status.ToString()
        };

        return Ok(response);
    }

    // GET /api/user-portal/private-stations
    // Eigene private Ladestationen des Benutzers
    [HttpGet("private-stations")]
    [SwaggerOperation(
        Summary = "Private Ladestationen abrufen",
        Description = "Ruft alle privaten Ladestationen ab, die dem eingeloggten Benutzer gehören."
    )]
    public async Task<IActionResult> GetPrivateStations()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var stations = await _context.ChargingStations
            .Where(s => s.IsPrivate && s.OwnerId == userId)
            .Select(s => new
            {
                s.Id,
                s.StationId,
                s.Name,
                Status = s.Status.ToString(),
                s.Vendor,
                s.Model,
                Type = s.Type.ToString(),
                s.MaxPower,
                s.NumberOfConnectors,
                s.Latitude,
                s.Longitude,
                s.Notes,
                s.ChargeBoxId,
                s.IsActive,
                s.CreatedAt,
                s.LastHeartbeat
            })
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(stations);
    }

    // POST /api/user-portal/private-stations
    // Neue private Ladestation anlegen
    [HttpPost("private-stations")]
    [SwaggerOperation(
        Summary = "Private Ladestation erstellen",
        Description = "Erstellt eine neue private Ladestation für den eingeloggten Benutzer."
    )]
    public async Task<IActionResult> CreatePrivateStation([FromBody] CreatePrivateStationDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var station = new ChargingStation
        {
            Id = Guid.NewGuid(),
            StationId = dto.StationId ?? $"HOME-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
            Name = dto.Name,
            Vendor = dto.Vendor ?? "Unknown",
            Model = dto.Model ?? "Unknown",
            Type = dto.Type,
            MaxPower = dto.MaxPower,
            NumberOfConnectors = dto.NumberOfConnectors,
            Status = ChargingStationStatus.Available,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Notes = dto.Notes,
            ChargeBoxId = dto.ChargeBoxId,
            OcppPassword = dto.OcppPassword,
            OcppProtocol = dto.OcppProtocol,
            OcppEndpoint = dto.OcppEndpoint,
            IsPrivate = true,
            OwnerId = userId,
            ChargingParkId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChargingStations.Add(station);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} created private charging station {StationId}", userId, station.Id);

        return CreatedAtAction(nameof(GetPrivateStation), new { id = station.Id }, new
        {
            station.Id,
            station.StationId,
            station.Name,
            Status = station.Status.ToString(),
            Type = station.Type.ToString(),
            station.MaxPower
        });
    }

    // GET /api/user-portal/private-stations/{id}
    // Details einer privaten Ladestation
    [HttpGet("private-stations/{id}")]
    [SwaggerOperation(
        Summary = "Private Ladestation abrufen",
        Description = "Ruft die Details einer privaten Ladestation ab."
    )]
    public async Task<IActionResult> GetPrivateStation(Guid id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var station = await _context.ChargingStations
            .Where(s => s.Id == id && s.IsPrivate && s.OwnerId == userId)
            .Select(s => new
            {
                s.Id,
                s.StationId,
                s.Name,
                Status = s.Status.ToString(),
                s.Vendor,
                s.Model,
                Type = s.Type.ToString(),
                s.MaxPower,
                s.NumberOfConnectors,
                s.Latitude,
                s.Longitude,
                s.Notes,
                s.ChargeBoxId,
                s.OcppProtocol,
                s.OcppEndpoint,
                s.IsActive,
                s.CreatedAt,
                s.LastHeartbeat
            })
            .FirstOrDefaultAsync();

        if (station == null)
            return NotFound(new { message = "Private Ladestation nicht gefunden" });

        return Ok(station);
    }

    // PUT /api/user-portal/private-stations/{id}
    // Private Ladestation aktualisieren
    [HttpPut("private-stations/{id}")]
    [SwaggerOperation(
        Summary = "Private Ladestation aktualisieren",
        Description = "Aktualisiert eine private Ladestation des Benutzers."
    )]
    public async Task<IActionResult> UpdatePrivateStation(Guid id, [FromBody] UpdatePrivateStationDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var station = await _context.ChargingStations
            .FirstOrDefaultAsync(s => s.Id == id && s.IsPrivate && s.OwnerId == userId);

        if (station == null)
            return NotFound(new { message = "Private Ladestation nicht gefunden" });

        // Update fields
        station.Name = dto.Name;
        station.Vendor = dto.Vendor ?? station.Vendor;
        station.Model = dto.Model ?? station.Model;
        station.Type = dto.Type;
        station.MaxPower = dto.MaxPower;
        station.NumberOfConnectors = dto.NumberOfConnectors;
        station.Latitude = dto.Latitude;
        station.Longitude = dto.Longitude;
        station.Notes = dto.Notes;
        station.ChargeBoxId = dto.ChargeBoxId;
        station.OcppPassword = dto.OcppPassword;
        station.OcppProtocol = dto.OcppProtocol;
        station.OcppEndpoint = dto.OcppEndpoint;
        station.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated private charging station {StationId}", userId, station.Id);

        return Ok(new
        {
            station.Id,
            station.Name,
            Message = "Ladestation erfolgreich aktualisiert"
        });
    }

    // DELETE /api/user-portal/private-stations/{id}
    // Private Ladestation löschen
    [HttpDelete("private-stations/{id}")]
    [SwaggerOperation(
        Summary = "Private Ladestation löschen",
        Description = "Löscht eine private Ladestation des Benutzers (Soft Delete)."
    )]
    public async Task<IActionResult> DeletePrivateStation(Guid id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var station = await _context.ChargingStations
            .FirstOrDefaultAsync(s => s.Id == id && s.IsPrivate && s.OwnerId == userId);

        if (station == null)
            return NotFound(new { message = "Private Ladestation nicht gefunden" });

        // Soft delete
        station.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deleted private charging station {StationId}", userId, station.Id);

        return NoContent();
    }

    // Helper method to get available stations count
    private async Task<int> GetAvailableStationsCount(Guid userId)
    {
        var userGroupIds = await _context.UserGroupMemberships
            .Where(m => m.UserId == userId)
            .Select(m => m.UserGroupId)
            .ToListAsync();

        var chargingStationGroupIds = await _context.UserGroupChargingStationGroupPermissions
            .Where(p => userGroupIds.Contains(p.UserGroupId))
            .Select(p => p.ChargingStationGroupId)
            .Distinct()
            .ToListAsync();

        var stationCount = await _context.ChargingStationGroupMemberships
            .Where(m => chargingStationGroupIds.Contains(m.ChargingStationGroupId))
            .Select(m => m.ChargingStationId)
            .Distinct()
            .CountAsync();

        // Add private stations count
        var privateStationsCount = await _context.ChargingStations
            .Where(s => s.IsPrivate && s.OwnerId == userId && s.IsActive)
            .CountAsync();

        return stationCount + privateStationsCount;
    }
}

// DTOs for private charging stations
public record CreatePrivateStationDto(
    string Name,
    ChargingStationType Type,
    int MaxPower,
    int NumberOfConnectors,
    string? StationId = null,
    string? Vendor = null,
    string? Model = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? Notes = null,
    string? ChargeBoxId = null,
    string? OcppPassword = null,
    string? OcppProtocol = null,
    string? OcppEndpoint = null
);

public record UpdatePrivateStationDto(
    string Name,
    ChargingStationType Type,
    int MaxPower,
    int NumberOfConnectors,
    bool IsActive,
    string? Vendor = null,
    string? Model = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? Notes = null,
    string? ChargeBoxId = null,
    string? OcppPassword = null,
    string? OcppProtocol = null,
    string? OcppEndpoint = null
);

