using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace ChargingControlSystem.Api.Controllers;

/// <summary>
/// Ladepunkte-Verwaltung (EVSE)
/// </summary>
[ApiController]
[Route("api/charging-points")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Verwaltung von Ladepunkten (EVSE) an Ladestationen")]
public class ChargingPointsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChargingPointsController> _logger;

    public ChargingPointsController(ApplicationDbContext context, ILogger<ChargingPointsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Alle Ladepunkte einer Station abrufen
    /// </summary>
    [HttpGet("station/{stationId}")]
    [SwaggerOperation(
        Summary = "Ladepunkte einer Station abrufen",
        Description = "Ruft alle Ladepunkte einer bestimmten Ladestation ab."
    )]
    public async Task<IActionResult> GetByStation(Guid stationId)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        // Prüfen, ob die Station zum Tenant gehört
        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.Id == stationId && s.ChargingPark.TenantId == tenantId.Value);

        if (station == null)
            return NotFound("Charging station not found");

        var points = await _context.ChargingPoints
            .Where(cp => cp.ChargingStationId == stationId && cp.IsActive)
            .Select(cp => new
            {
                cp.Id,
                cp.EvseId,
                cp.EvseIdExternal,
                cp.Name,
                cp.Description,
                cp.MaxPower,
                Status = cp.Status.ToString(),
                cp.ConnectorType,
                cp.ConnectorFormat,
                cp.PowerType,
                cp.MaxCurrent,
                cp.MaxVoltage,
                cp.PhysicalReference,
                cp.PublicKey,
                cp.SupportsSmartCharging,
                cp.SupportsRemoteStartStop,
                cp.SupportsReservation,
                cp.TariffInfo,
                cp.Notes,
                cp.IsActive,
                cp.CreatedAt,
                cp.LastStatusChange
            })
            .ToListAsync();

        return Ok(points);
    }

    /// <summary>
    /// Einzelnen Ladepunkt abrufen
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Ladepunkt abrufen",
        Description = "Ruft einen einzelnen Ladepunkt mit allen Details ab."
    )]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var point = await _context.ChargingPoints
            .Include(cp => cp.ChargingStation)
                .ThenInclude(s => s.ChargingPark)
            .Where(cp => cp.Id == id && cp.ChargingStation.ChargingPark.TenantId == tenantId.Value)
            .Select(cp => new
            {
                cp.Id,
                cp.ChargingStationId,
                cp.EvseId,
                cp.EvseIdExternal,
                cp.Name,
                cp.Description,
                cp.MaxPower,
                Status = cp.Status.ToString(),
                cp.ConnectorType,
                cp.ConnectorFormat,
                cp.PowerType,
                cp.MaxCurrent,
                cp.MaxVoltage,
                cp.PhysicalReference,
                cp.PublicKey,
                cp.CertificateChain,
                cp.SupportsSmartCharging,
                cp.SupportsRemoteStartStop,
                cp.SupportsReservation,
                cp.TariffInfo,
                cp.Notes,
                cp.IsActive,
                cp.CreatedAt,
                cp.LastStatusChange
            })
            .FirstOrDefaultAsync();

        if (point == null)
            return NotFound("Charging point not found");

        return Ok(point);
    }

    /// <summary>
    /// Neuen Ladepunkt erstellen
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Ladepunkt erstellen",
        Description = "Erstellt einen neuen Ladepunkt an einer Ladestation."
    )]
    public async Task<IActionResult> Create([FromBody] CreateChargingPointDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        // Prüfen, ob die Station zum Tenant gehört
        var station = await _context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.Id == dto.ChargingStationId && s.ChargingPark.TenantId == tenantId.Value);

        if (station == null)
            return NotFound("Charging station not found");

        // Prüfen, ob EvseId bereits existiert
        var existingEvseId = await _context.ChargingPoints
            .AnyAsync(cp => cp.ChargingStationId == dto.ChargingStationId && cp.EvseId == dto.EvseId);

        if (existingEvseId)
            return BadRequest($"EVSE ID {dto.EvseId} already exists for this station");

        // Prüfen, ob externe EVSE-ID bereits existiert
        if (!string.IsNullOrEmpty(dto.EvseIdExternal))
        {
            var existingExternalId = await _context.ChargingPoints
                .AnyAsync(cp => cp.EvseIdExternal == dto.EvseIdExternal);

            if (existingExternalId)
                return BadRequest($"External EVSE ID {dto.EvseIdExternal} already exists");
        }

        var point = new ChargingPoint
        {
            Id = Guid.NewGuid(),
            ChargingStationId = dto.ChargingStationId,
            EvseId = dto.EvseId,
            EvseIdExternal = dto.EvseIdExternal,
            Name = dto.Name,
            Description = dto.Description,
            MaxPower = dto.MaxPower,
            ConnectorType = dto.ConnectorType ?? "Type2",
            ConnectorFormat = dto.ConnectorFormat,
            PowerType = dto.PowerType,
            MaxCurrent = dto.MaxCurrent ?? 32,
            MaxVoltage = dto.MaxVoltage ?? 230,
            PhysicalReference = dto.PhysicalReference,
            Status = dto.Status,
            PublicKey = dto.PublicKey,
            CertificateChain = dto.CertificateChain,
            SupportsSmartCharging = dto.SupportsSmartCharging,
            SupportsRemoteStartStop = dto.SupportsRemoteStartStop,
            SupportsReservation = dto.SupportsReservation,
            TariffInfo = dto.TariffInfo,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChargingPoints.Add(point);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created charging point {EvseId} at station {StationId}", dto.EvseId, dto.ChargingStationId);

        return CreatedAtAction(nameof(GetById), new { id = point.Id }, new
        {
            point.Id,
            point.ChargingStationId,
            point.EvseId,
            point.EvseIdExternal,
            point.Name,
            point.Description,
            point.MaxPower,
            Status = point.Status.ToString(),
            point.SupportsSmartCharging,
            point.SupportsRemoteStartStop,
            point.SupportsReservation,
            point.IsActive,
            point.CreatedAt
        });
    }

    /// <summary>
    /// Ladepunkt aktualisieren
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Ladepunkt aktualisieren",
        Description = "Aktualisiert die Daten eines Ladepunkts."
    )]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChargingPointDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var point = await _context.ChargingPoints
            .Include(cp => cp.ChargingStation)
                .ThenInclude(s => s.ChargingPark)
            .FirstOrDefaultAsync(cp => cp.Id == id && cp.ChargingStation.ChargingPark.TenantId == tenantId.Value);

        if (point == null)
            return NotFound("Charging point not found");

        // Prüfen, ob EvseId geändert werden soll und ob sie verfügbar ist
        if (dto.EvseId != point.EvseId)
        {
            var existingEvseId = await _context.ChargingPoints
                .AnyAsync(cp => cp.ChargingStationId == point.ChargingStationId && 
                               cp.EvseId == dto.EvseId && 
                               cp.Id != id);

            if (existingEvseId)
                return BadRequest($"EVSE ID {dto.EvseId} already exists for this station");
        }

        // Prüfen, ob externe EVSE-ID geändert werden soll und ob sie verfügbar ist
        if (!string.IsNullOrEmpty(dto.EvseIdExternal) && dto.EvseIdExternal != point.EvseIdExternal)
        {
            var existingExternalId = await _context.ChargingPoints
                .AnyAsync(cp => cp.EvseIdExternal == dto.EvseIdExternal && cp.Id != id);

            if (existingExternalId)
                return BadRequest($"External EVSE ID {dto.EvseIdExternal} already exists");
        }

        // Update
        point.EvseId = dto.EvseId;
        point.EvseIdExternal = dto.EvseIdExternal;
        point.Name = dto.Name;
        point.Description = dto.Description;
        point.MaxPower = dto.MaxPower;
        if (!string.IsNullOrEmpty(dto.ConnectorType))
            point.ConnectorType = dto.ConnectorType;
        point.ConnectorFormat = dto.ConnectorFormat;
        point.PowerType = dto.PowerType;
        if (dto.MaxCurrent.HasValue)
            point.MaxCurrent = dto.MaxCurrent.Value;
        if (dto.MaxVoltage.HasValue)
            point.MaxVoltage = dto.MaxVoltage.Value;
        point.PhysicalReference = dto.PhysicalReference;
        point.Status = dto.Status;
        point.PublicKey = dto.PublicKey;
        point.CertificateChain = dto.CertificateChain;
        point.SupportsSmartCharging = dto.SupportsSmartCharging;
        point.SupportsRemoteStartStop = dto.SupportsRemoteStartStop;
        point.SupportsReservation = dto.SupportsReservation;
        point.TariffInfo = dto.TariffInfo;
        point.Notes = dto.Notes;
        point.LastStatusChange = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated charging point {Id} (EVSE {EvseId})", id, dto.EvseId);

        return Ok(new
        {
            point.Id,
            point.ChargingStationId,
            point.EvseId,
            point.EvseIdExternal,
            point.Name,
            point.Description,
            point.MaxPower,
            Status = point.Status.ToString(),
            point.SupportsSmartCharging,
            point.SupportsRemoteStartStop,
            point.SupportsReservation,
            point.IsActive,
            point.LastStatusChange
        });
    }

    /// <summary>
    /// Ladepunkt löschen
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Ladepunkt löschen",
        Description = "Löscht einen Ladepunkt (soft delete - setzt IsActive auf false)."
    )]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var point = await _context.ChargingPoints
            .Include(cp => cp.ChargingStation)
                .ThenInclude(s => s.ChargingPark)
            .FirstOrDefaultAsync(cp => cp.Id == id && cp.ChargingStation.ChargingPark.TenantId == tenantId.Value);

        if (point == null)
            return NotFound("Charging point not found");

        // Prüfen, ob es aktive Ladevorgänge gibt
        var hasActiveSessions = await _context.ChargingSessions
            .AnyAsync(s => s.ChargingPointId == id && s.EndedAt == null);

        if (hasActiveSessions)
            return BadRequest("Cannot delete charging point with active charging sessions");

        // Soft delete: Ladepunkt deaktivieren
        point.IsActive = false;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted charging point {Id} (EVSE {EvseId})", id, point.EvseId);

        return Ok(new { message = "Charging point deleted successfully" });
    }
}

// DTOs
public class CreateChargingPointDto
{
    public Guid ChargingStationId { get; set; }
    public int EvseId { get; set; }
    public string? EvseIdExternal { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPower { get; set; }
    public string? ConnectorType { get; set; }
    public string? ConnectorFormat { get; set; }
    public string? PowerType { get; set; }
    public int? MaxCurrent { get; set; }
    public int? MaxVoltage { get; set; }
    public string? PhysicalReference { get; set; }
    public ChargingPointStatus Status { get; set; } = ChargingPointStatus.Available;
    public string? PublicKey { get; set; }
    public string? CertificateChain { get; set; }
    public bool SupportsSmartCharging { get; set; } = false;
    public bool SupportsRemoteStartStop { get; set; } = true;
    public bool SupportsReservation { get; set; } = false;
    public string? TariffInfo { get; set; }
    public string? Notes { get; set; }
}

public class UpdateChargingPointDto
{
    public int EvseId { get; set; }
    public string? EvseIdExternal { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPower { get; set; }
    public string? ConnectorType { get; set; }
    public string? ConnectorFormat { get; set; }
    public string? PowerType { get; set; }
    public int? MaxCurrent { get; set; }
    public int? MaxVoltage { get; set; }
    public string? PhysicalReference { get; set; }
    public ChargingPointStatus Status { get; set; }
    public string? PublicKey { get; set; }
    public string? CertificateChain { get; set; }
    public bool SupportsSmartCharging { get; set; }
    public bool SupportsRemoteStartStop { get; set; }
    public bool SupportsReservation { get; set; }
    public string? TariffInfo { get; set; }
    public string? Notes { get; set; }
}


