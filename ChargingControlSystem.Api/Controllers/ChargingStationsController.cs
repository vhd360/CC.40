using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

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

    public ChargingStationsController(ApplicationDbContext context)
    {
        _context = context;
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
            .Where(s => !s.IsPrivate && s.ChargingPark.TenantId == tenantId.Value && s.IsActive)
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
                s.LastHeartbeat,
                s.ChargeBoxId,
                s.OcppProtocol,
                s.CreatedAt
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
                .ThenInclude(cp => cp.Connectors)
            .Where(s => s.Id == id && !s.IsPrivate && s.ChargingPark.TenantId == tenantId.Value)
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
                    cp.Name,
                    cp.MaxPower,
                    Status = cp.Status.ToString(),
                    Connectors = cp.Connectors.Select(c => new
                    {
                        c.Id,
                        c.ConnectorId,
                        ConnectorType = c.ConnectorType.ToString(),
                        Status = c.Status.ToString(),
                        c.MaxPower,
                        c.MaxCurrent,
                        c.MaxVoltage
                    }).ToList()
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

        // Private stations can only be updated via UserPortalController
        if (station.IsPrivate)
            return BadRequest("Private charging stations cannot be updated through this endpoint");

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

        // Private stations can only be deleted via UserPortalController
        if (station.IsPrivate)
            return BadRequest("Private charging stations cannot be deleted through this endpoint");

        station.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
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
