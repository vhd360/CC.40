using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using ChargingControlSystem.Api.Services;
using ChargingControlSystem.OCPP.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace ChargingControlSystem.Api.Controllers;

/// <summary>
/// Ladesäulen-Verwaltung
/// </summary>
[ApiController]
[Route("api/charging-stations")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Verwaltung von Ladesäulen, Ladepunkten (EVSEs) und Konnektoren")]
public class ChargingStationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IOcppCommandService _ocppCommandService;
    private readonly ILogger<ChargingStationsController> _logger;

    public ChargingStationsController(
        ApplicationDbContext context, 
        IOcppCommandService ocppCommandService,
        ILogger<ChargingStationsController> logger)
    {
        _context = context;
        _ocppCommandService = ocppCommandService;
        _logger = logger;
    }

    /// <summary>
    /// Alle Ladesäulen abrufen
    /// </summary>
    /// <returns>Liste aller Ladesäulen des aktuellen Tenants</returns>
    /// <remarks>
    /// Gibt eine vollständige Liste aller aktiven Ladesäulen zurück, inklusive:
    /// - **Grunddaten**: Name, Status, Typ, Hersteller, Modell
    /// - **Leistungsdaten**: Maximale Leistung, Anzahl Konnektoren
    /// - **OCPP**: ChargeBoxId, Protokoll, letzter Heartbeat
    /// - **Gruppen**: Zugehörige Ladesäulen-Gruppen
    /// - **Standort**: Zugeordneter Ladepark
    /// 
    /// Die Daten werden automatisch nach dem Tenant des eingeloggten Benutzers gefiltert.
    /// </remarks>
    /// <response code="200">Liste der Ladesäulen erfolgreich abgerufen</response>
    /// <response code="400">Tenant nicht gefunden</response>
    /// <response code="401">Nicht autorisiert</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Alle Ladesäulen abrufen",
        Description = "Ruft alle aktiven Ladesäulen des aktuellen Tenants ab, inklusive zugehöriger Ladeparks und Gruppen.",
        OperationId = "GetAllChargingStations",
        Tags = new[] { "Ladesäulen" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var stations = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .Where(s => s.ChargingPark.TenantId == tenantId.Value && s.IsActive)
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
                ChargingPark = new
                {
                    s.ChargingPark.Id,
                    s.ChargingPark.Name
                },
                Groups = _context.ChargingStationGroupMemberships
                    .Where(m => m.ChargingStationId == s.Id)
                    .Select(m => new
                    {
                        m.ChargingStationGroup.Id,
                        m.ChargingStationGroup.Name
                    })
                    .ToList(),
                LastHeartbeat = s.LastHeartbeat.HasValue ? DateTime.SpecifyKind(s.LastHeartbeat.Value, DateTimeKind.Utc) : (DateTime?)null,
                s.ChargeBoxId,
                s.OcppProtocol,
                CreatedAt = DateTime.SpecifyKind(s.CreatedAt, DateTimeKind.Utc)
            })
            .ToListAsync();

        return Ok(stations);
    }

    /// <summary>
    /// Ladesäule nach ID abrufen
    /// </summary>
    /// <param name="id">Eindeutige ID der Ladesäule</param>
    /// <returns>Detaillierte Informationen zur Ladesäule</returns>
    /// <remarks>
    /// Gibt detaillierte Informationen zu einer Ladesäule zurück, inklusive:
    /// - **Vollständige Stammdaten**
    /// - **OCPP-Konfiguration**: ChargeBoxId, Passwort, Endpoint, Protokoll
    /// - **Hierarchie**: Ladepunkte (EVSEs) mit ihren Konnektoren
    /// - **Gruppen**: Alle zugeordneten Ladesäulen-Gruppen
    /// - **Geo-Daten**: Latitude, Longitude
    /// 
    /// Wird für die Detailansicht und Bearbeitung verwendet.
    /// </remarks>
    /// <response code="200">Ladesäule gefunden</response>
    /// <response code="400">Tenant nicht gefunden</response>
    /// <response code="404">Ladesäule nicht gefunden</response>
    /// <response code="401">Nicht autorisiert</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Ladesäule nach ID abrufen",
        Description = "Ruft eine einzelne Ladesäule mit vollständigen Details, Ladepunkten und Konnektoren ab.",
        OperationId = "GetChargingStationById",
        Tags = new[] { "Ladesäulen" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .Include(s => s.ChargingPoints)
            .Where(s => s.Id == id && s.ChargingPark.TenantId == tenantId.Value)
            .Select(s => new
            {
                s.Id,
                s.StationId,
                s.Name,
                s.Vendor,
                s.Model,
                Type = s.Type.ToString(),
                Status = s.Status.ToString(),
                s.MaxPower,
                s.NumberOfConnectors,
                s.Latitude,
                s.Longitude,
                s.Notes,
                s.IsActive,
                s.CreatedAt,
                s.LastHeartbeat,
                // OCPP Fields
                s.ChargeBoxId,
                s.OcppPassword,
                s.OcppProtocol,
                s.OcppEndpoint,
                ChargingPark = new
                {
                    s.ChargingPark.Id,
                    s.ChargingPark.Name,
                    s.ChargingPark.Address,
                    s.ChargingPark.City
                },
                ChargingPoints = s.ChargingPoints.Select(cp => new
                {
                    cp.Id,
                    cp.EvseId,
                    cp.EvseIdExternal,
                    cp.Name,
                    cp.Description,
                    cp.MaxPower,
                    Status = cp.Status.ToString(),
                    ConnectorType = cp.ConnectorType,
                    ConnectorFormat = cp.ConnectorFormat,
                    PowerType = cp.PowerType,
                    MaxCurrent = cp.MaxCurrent,
                    MaxVoltage = cp.MaxVoltage,
                    PhysicalReference = cp.PhysicalReference,
                    SupportsSmartCharging = cp.SupportsSmartCharging,
                    SupportsRemoteStartStop = cp.SupportsRemoteStartStop,
                    SupportsReservation = cp.SupportsReservation,
                    PublicKey = cp.PublicKey,
                    CertificateChain = cp.CertificateChain,
                    TariffInfo = cp.TariffInfo,
                    Notes = cp.Notes,
                    IsActive = cp.IsActive,
                    CreatedAt = cp.CreatedAt,
                    LastStatusChange = cp.LastStatusChange
                }).ToList(),
                Groups = _context.ChargingStationGroupMemberships
                    .Where(m => m.ChargingStationId == s.Id)
                    .Select(m => new
                    {
                        m.ChargingStationGroup.Id,
                        m.ChargingStationGroup.Name,
                        m.ChargingStationGroup.Description,
                        m.AssignedAt
                    })
                    .ToList(),
                AvailableGroups = _context.ChargingStationGroups
                    .Where(g => g.TenantId == tenantId.Value && 
                                g.IsActive && 
                                !_context.ChargingStationGroupMemberships.Any(m => m.ChargingStationId == s.Id && m.ChargingStationGroupId == g.Id))
                    .Select(g => new { g.Id, g.Name })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (station == null)
            return NotFound();

        return Ok(station);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChargingStationDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var park = await _context.ChargingParks
            .FirstOrDefaultAsync(p => p.Id == dto.ChargingParkId && p.TenantId == tenantId.Value);

        if (park == null)
            return NotFound("Charging park not found");

        var station = new ChargingStation
        {
            Id = Guid.NewGuid(),
            ChargingParkId = dto.ChargingParkId,
            StationId = dto.StationId,
            Name = dto.Name,
            Vendor = dto.Vendor,
            Model = dto.Model,
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
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChargingStations.Add(station);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = station.Id }, new { station.Id, station.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChargingStationDto dto)
    {
        var station = await _context.ChargingStations.FindAsync(id);
        if (station == null)
            return NotFound();

        station.StationId = dto.StationId;
        station.Name = dto.Name;
        station.Vendor = dto.Vendor;
        station.Model = dto.Model;
        station.Type = dto.Type;
        station.MaxPower = dto.MaxPower;
        station.NumberOfConnectors = dto.NumberOfConnectors;
        station.Status = dto.Status;
        station.Latitude = dto.Latitude;
        station.Longitude = dto.Longitude;
        station.Notes = dto.Notes;
        station.ChargeBoxId = dto.ChargeBoxId;
        station.OcppPassword = dto.OcppPassword;
        station.OcppProtocol = dto.OcppProtocol;
        station.OcppEndpoint = dto.OcppEndpoint;
        station.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { station.Id, station.Name });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var station = await _context.ChargingStations.FindAsync(id);
        if (station == null)
            return NotFound();

        station.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Konfiguration einer Ladesäule abrufen
    /// </summary>
    [HttpGet("{id}/configuration")]
    [SwaggerOperation(Summary = "Konfiguration abrufen", OperationId = "GetStationConfiguration")]
    public async Task<ActionResult<GetConfigurationResponse>> GetConfiguration(Guid id)
    {
        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (station == null || station.ChargingPark.TenantId != GetCurrentTenantId())
            return NotFound();

        if (string.IsNullOrEmpty(station.ChargeBoxId))
            return BadRequest(new { error = "Station hat keine ChargeBoxId. Bitte konfigurieren Sie die OCPP-Verbindung für diese Station." });

        // Request configuration from station
        var previousUpdateTime = station.LastConfigurationUpdate;
        try
        {
            await _ocppCommandService.GetConfigurationAsync(station.ChargeBoxId);
            
            // Wait a bit for the response to arrive and be processed (max 3 seconds)
            var maxWaitTime = TimeSpan.FromSeconds(3);
            var checkInterval = TimeSpan.FromMilliseconds(200);
            var waited = TimeSpan.Zero;
            
            while (waited < maxWaitTime)
            {
                await Task.Delay(checkInterval);
                waited += checkInterval;
                
                // Reload station to check if configuration was updated
                await _context.Entry(station).ReloadAsync();
                
                if (station.LastConfigurationUpdate > previousUpdateTime)
                {
                    _logger.LogInformation("Configuration received for {ChargeBoxId} after {WaitTime}ms", 
                        station.ChargeBoxId, waited.TotalMilliseconds);
                    break;
                }
            }
            
            if (waited >= maxWaitTime)
            {
                _logger.LogWarning("Timeout waiting for GetConfiguration response from {ChargeBoxId}", station.ChargeBoxId);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No active connection"))
        {
            // Station is not connected - return stored configuration if available
            _logger.LogWarning("Station {ChargeBoxId} is not connected. Returning stored configuration.", station.ChargeBoxId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending GetConfiguration to station {ChargeBoxId}", station.ChargeBoxId);
            return BadRequest(new { error = $"Fehler beim Senden des GetConfiguration-Commands: {ex.Message}" });
        }

        // Return stored configuration (may have been updated by the response handler)
        var configurationKeys = new List<ConfigurationKey>();
        if (!string.IsNullOrEmpty(station.ConfigurationJson))
        {
            try
            {
                var storedConfig = JsonConvert.DeserializeObject<List<ConfigurationKey>>(station.ConfigurationJson);
                if (storedConfig != null)
                {
                    configurationKeys = storedConfig;
                }
            }
            catch
            {
                // Ignore deserialization errors
            }
        }

        return Ok(new GetConfigurationResponse
        {
            ConfigurationKey = configurationKeys,
            UnknownKey = null
        });
    }

    /// <summary>
    /// Konfiguration einer Ladesäule ändern
    /// </summary>
    [HttpPost("{id}/configuration")]
    [SwaggerOperation(Summary = "Konfiguration ändern", OperationId = "ChangeStationConfiguration")]
    public async Task<ActionResult<ChangeConfigurationResponse>> ChangeConfiguration(
        Guid id, 
        [FromBody] ChangeConfigurationRequest request)
    {
        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (station == null || station.ChargingPark.TenantId != GetCurrentTenantId())
            return NotFound();

        if (string.IsNullOrEmpty(station.ChargeBoxId))
            return BadRequest("Station hat keine ChargeBoxId");

        try
        {
            var response = await _ocppCommandService.ChangeConfigurationAsync(
                station.ChargeBoxId, 
                request.Key, 
                request.Value);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest($"Fehler beim Senden des ChangeConfiguration-Commands: {ex.Message}");
        }
    }

    /// <summary>
    /// Diagnoseinformationen anfordern
    /// </summary>
    [HttpPost("{id}/diagnostics")]
    [SwaggerOperation(Summary = "Diagnoseinformationen anfordern", OperationId = "RequestStationDiagnostics")]
    public async Task<ActionResult<GetDiagnosticsResponse>> RequestDiagnostics(
        Guid id,
        [FromBody] RequestDiagnosticsDto request)
    {
        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (station == null || station.ChargingPark.TenantId != GetCurrentTenantId())
            return NotFound();

        if (string.IsNullOrEmpty(station.ChargeBoxId))
            return BadRequest("Station hat keine ChargeBoxId");

        try
        {
            var response = await _ocppCommandService.RequestDiagnosticsAsync(
                station.ChargeBoxId,
                request.Location,
                request.StartTime,
                request.StopTime);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest($"Fehler beim Senden des GetDiagnostics-Commands: {ex.Message}");
        }
    }

    /// <summary>
    /// Firmware-Historie abrufen
    /// </summary>
    [HttpGet("{id}/firmware-history")]
    [SwaggerOperation(Summary = "Firmware-Historie abrufen", OperationId = "GetFirmwareHistory")]
    public async Task<ActionResult<List<FirmwareHistoryDto>>> GetFirmwareHistory(Guid id)
    {
        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (station == null || station.ChargingPark.TenantId != GetCurrentTenantId())
            return NotFound();

        var history = await _context.ChargingStationFirmwareHistory
            .Where(h => h.ChargingStationId == id)
            .OrderByDescending(h => h.Timestamp)
            .Select(h => new FirmwareHistoryDto(
                h.Id,
                h.FirmwareVersion,
                h.Status,
                h.Info,
                h.Timestamp
            ))
            .ToListAsync();

        return Ok(history);
    }

    /// <summary>
    /// Diagnose-Historie abrufen
    /// </summary>
    [HttpGet("{id}/diagnostics-history")]
    [SwaggerOperation(Summary = "Diagnose-Historie abrufen", OperationId = "GetDiagnosticsHistory")]
    public async Task<ActionResult<List<DiagnosticsHistoryDto>>> GetDiagnosticsHistory(Guid id)
    {
        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (station == null || station.ChargingPark.TenantId != GetCurrentTenantId())
            return NotFound();

        var history = await _context.ChargingStationDiagnostics
            .Where(d => d.ChargingStationId == id)
            .OrderByDescending(d => d.RequestedAt)
            .Select(d => new DiagnosticsHistoryDto(
                d.Id,
                d.RequestedAt,
                d.CompletedAt,
                d.Status,
                d.FileName,
                d.DiagnosticsUrl,
                d.ErrorMessage,
                d.StartTime,
                d.StopTime
            ))
            .ToListAsync();

        return Ok(history);
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return Guid.Parse(tenantIdClaim ?? throw new UnauthorizedAccessException("TenantId not found"));
    }
}

public record CreateChargingStationDto(
    Guid ChargingParkId,
    string StationId,
    string Name,
    string Vendor,
    string Model,
    ChargingStationType Type,
    int MaxPower,
    int NumberOfConnectors,
    decimal? Latitude,
    decimal? Longitude,
    string? Notes,
    string? ChargeBoxId,
    string? OcppPassword,
    string? OcppProtocol,
    string? OcppEndpoint
);

public record UpdateChargingStationDto(
    string StationId,
    string Name,
    string Vendor,
    string Model,
    ChargingStationType Type,
    int MaxPower,
    int NumberOfConnectors,
    ChargingStationStatus Status,
    decimal? Latitude,
    decimal? Longitude,
    string? Notes,
    string? ChargeBoxId,
    string? OcppPassword,
    string? OcppProtocol,
    string? OcppEndpoint,
    bool IsActive
);

public record RequestDiagnosticsDto(
    string Location,
    DateTime? StartTime = null,
    DateTime? StopTime = null
);

public record FirmwareHistoryDto(
    Guid Id,
    string FirmwareVersion,
    string Status,
    string? Info,
    DateTime Timestamp
);

public record DiagnosticsHistoryDto(
    Guid Id,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    string Status,
    string? FileName,
    string? DiagnosticsUrl,
    string? ErrorMessage,
    DateTime? StartTime,
    DateTime? StopTime
);
