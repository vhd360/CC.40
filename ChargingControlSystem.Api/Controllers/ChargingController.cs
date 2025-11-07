using ChargingControlSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingControlSystem.Api.Controllers;

/// <summary>
/// Ladevorgänge und Sessions
/// </summary>
[ApiController]
[Route("api/charging")]
// [Authorize] // Temporarily disabled for development
[Produces("application/json")]
[SwaggerTag("Verwaltung von Ladevorgängen und Lade-Sessions")]
public class ChargingController : ControllerBase
{
    private readonly IChargingService _chargingService;

    public ChargingController(IChargingService chargingService)
    {
        _chargingService = chargingService;
    }

    /// <summary>
    /// Alle verfügbaren Ladesäulen abrufen
    /// </summary>
    /// <returns>Liste aller aktiven Ladesäulen mit Standort und Status</returns>
    /// <remarks>
    /// Gibt eine Liste aller Ladesäulen zurück mit:
    /// - **Standortdaten**: Latitude, Longitude für Karten-Darstellung
    /// - **Status**: Verfügbarkeit der Ladesäule
    /// - **Technische Daten**: Leistung, Konnektoren, Typ
    /// - **OCPP**: Letzter Heartbeat
    /// 
    /// Wird für die Karten-Ansicht und Station-Auswahl verwendet.
    /// </remarks>
    /// <response code="200">Liste der Ladesäulen erfolgreich abgerufen</response>
    [HttpGet("stations")]
    [SwaggerOperation(
        Summary = "Alle Ladesäulen abrufen",
        Description = "Ruft alle verfügbaren Ladesäulen mit Standortdaten und Status ab.",
        OperationId = "GetChargingStations",
        Tags = new[] { "Ladevorgänge" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChargingStations()
    {
        var stations = await _chargingService.GetChargingStationsAsync();
        var result = stations.Select(s => new
        {
            s.Id,
            s.StationId,
            s.Name,
            Status = s.Status.ToString().ToLower(),
            s.Latitude,
            s.Longitude,
            s.MaxPower,
            s.NumberOfConnectors,
            s.Vendor,
            s.Model,
            Type = s.Type.ToString(),
            s.CreatedAt,
            s.LastHeartbeat
        });
        return Ok(result);
    }

    /// <summary>
    /// Ladevorgang starten
    /// </summary>
    /// <param name="connectorId">ID des Konnektors</param>
    /// <param name="vehicleId">ID des Fahrzeugs (optional)</param>
    /// <returns>Neue Lade-Session</returns>
    /// <remarks>
    /// Startet einen neuen Ladevorgang an einem Konnektor.
    /// 
    /// **Voraussetzungen:**
    /// - Konnektor muss verfügbar sein (Status: Available)
    /// - Benutzer muss berechtigt sein (via User Group Permissions)
    /// - Bei OCPP-Stationen wird RemoteStartTransaction gesendet
    /// 
    /// **Response enthält:**
    /// - Session-ID für späteren Stop
    /// - Startzeitpunkt
    /// - Konnektor- und Fahrzeugdaten
    /// 
    /// Die Session wird automatisch beendet, wenn:
    /// - Manuell gestoppt via `/api/charging/stop/{sessionId}`
    /// - Ladesäule meldet StopTransaction via OCPP
    /// - Maximale Session-Dauer erreicht
    /// </remarks>
    /// <response code="200">Ladevorgang erfolgreich gestartet</response>
    /// <response code="400">Konnektor nicht verfügbar oder ungültig</response>
    /// <response code="403">Keine Berechtigung für diese Ladesäule</response>
    [HttpPost("start/{connectorId}")]
    [SwaggerOperation(
        Summary = "Ladevorgang starten",
        Description = "Startet einen neuen Ladevorgang an einem bestimmten Konnektor.",
        OperationId = "StartCharging",
        Tags = new[] { "Ladevorgänge" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartCharging(Guid connectorId, [FromQuery] Guid? vehicleId)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;

        var session = await _chargingService.StartChargingSessionAsync(connectorId, userId, vehicleId);
        
        // Return DTO to avoid circular reference issues
        return Ok(new
        {
            session.Id,
            session.SessionId,
            session.StartedAt,
            session.ChargingConnectorId,
            session.UserId,
            session.VehicleId,
            Status = session.Status.ToString()
        });
    }

    /// <summary>
    /// Ladevorgang beenden
    /// </summary>
    /// <param name="sessionId">ID der Lade-Session</param>
    /// <returns>Beendete Session mit Abrechnung</returns>
    /// <remarks>
    /// Beendet eine laufende Lade-Session.
    /// 
    /// **Aktionen:**
    /// - Setzt EndedAt-Zeitstempel
    /// - Berechnet Dauer und Energiemenge
    /// - Erstellt Abrechnungsdaten (Cost)
    /// - Bei OCPP-Stationen: Sendet RemoteStopTransaction
    /// 
    /// **Response enthält:**
    /// - Vollständige Session-Daten
    /// - Dauer und Energiemenge
    /// - Kosten
    /// 
    /// Sessions können auch automatisch durch die Ladesäule
    /// (via OCPP StopTransaction) beendet werden.
    /// </remarks>
    /// <response code="200">Ladevorgang erfolgreich beendet</response>
    /// <response code="404">Session nicht gefunden</response>
    /// <response code="400">Session bereits beendet</response>
    [HttpPost("stop/{sessionId}")]
    [SwaggerOperation(
        Summary = "Ladevorgang beenden",
        Description = "Beendet eine laufende Lade-Session und erstellt die Abrechnung.",
        OperationId = "StopCharging",
        Tags = new[] { "Ladevorgänge" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StopCharging(Guid sessionId)
    {
        var session = await _chargingService.StopChargingSessionAsync(sessionId);
        
        // Return DTO to avoid circular reference issues
        return Ok(new
        {
            session.Id,
            session.SessionId,
            session.StartedAt,
            session.EndedAt,
            DurationMinutes = session.EndedAt.HasValue 
                ? (int)(session.EndedAt.Value - session.StartedAt).TotalMinutes 
                : 0,
            session.EnergyDelivered,
            session.Cost,
            Status = session.Status.ToString()
        });
    }

    /// <summary>
    /// Lade-Sessions abrufen
    /// </summary>
    /// <returns>Liste aller Lade-Sessions des aktuellen Benutzers</returns>
    /// <remarks>
    /// Gibt die Lade-History des Benutzers zurück.
    /// 
    /// **Enthaltene Daten:**
    /// - Benutzer- und Fahrzeugdaten
    /// - Ladesäule und Konnektor
    /// - Start- und Endzeitpunkt
    /// - Dauer und Status
    /// - Kosten
    /// 
    /// **Session-Status:**
    /// - `Charging`: Ladevorgang läuft aktuell
    /// - `Completed`: Ladevorgang erfolgreich beendet
    /// - `Failed`: Ladevorgang mit Fehler beendet
    /// - `Cancelled`: Ladevorgang abgebrochen
    /// 
    /// Sessions werden absteigend nach Startzeit sortiert.
    /// </remarks>
    /// <response code="200">Sessions erfolgreich abgerufen</response>
    [HttpGet("sessions")]
    [SwaggerOperation(
        Summary = "Lade-Sessions abrufen",
        Description = "Ruft die Lade-History des aktuellen Benutzers ab.",
        OperationId = "GetChargingSessions",
        Tags = new[] { "Ladevorgänge" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChargingSessions()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;

        var sessions = await _chargingService.GetChargingSessionsAsync(userId);
        var result = sessions.Select(s => new
        {
            s.Id,
            User = s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : "Adhoc-Nutzer",
            Vehicle = s.Vehicle != null ? $"{s.Vehicle.Make} {s.Vehicle.Model}" : "Unbekannt",
            Station = s.ChargingConnector.ChargingPoint.ChargingStation.Name,
            Duration = s.EndedAt.HasValue 
                ? $"{(int)(s.EndedAt.Value - s.StartedAt).TotalMinutes} min" 
                : "Läuft...",
            Cost = $"€{s.Cost:F2}",
            Status = s.Status.ToString().ToLower(),
            s.StartedAt
        });
        return Ok(result);
    }

    /// <summary>
    /// Aktive Sessions des aktuellen Benutzers abrufen
    /// </summary>
    [HttpGet("sessions/active")]
    [SwaggerOperation(
        Summary = "Aktive Lade-Sessions abrufen",
        Description = "Ruft alle derzeit laufenden Lade-Sessions des aktuellen Benutzers ab.",
        OperationId = "GetActiveSessions",
        Tags = new[] { "Ladevorgänge" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;

        if (!userId.HasValue)
            return Unauthorized();

        var sessions = await _chargingService.GetActiveSessionsForUserAsync(userId.Value);
        return Ok(sessions);
    }

    /// <summary>
    /// Verfügbare Connectoren für eine Station abrufen
    /// </summary>
    [HttpGet("stations/{stationId}/connectors")]
    [SwaggerOperation(
        Summary = "Verfügbare Connectoren abrufen",
        Description = "Ruft alle Connectoren einer Station mit ihrem Status ab.",
        OperationId = "GetStationConnectors",
        Tags = new[] { "Ladevorgänge" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStationConnectors(Guid stationId)
    {
        var connectors = await _chargingService.GetStationConnectorsAsync(stationId);
        return Ok(connectors);
    }

    /// <summary>
    /// Connector-Status zurücksetzen (Admin)
    /// </summary>
    [HttpPost("connectors/{connectorId}/reset")]
    [SwaggerOperation(
        Summary = "Connector-Status zurücksetzen",
        Description = "Setzt den Status eines blockierten Connectors auf 'Available' zurück. Nur möglich, wenn keine aktive Session existiert.",
        OperationId = "ResetConnectorStatus",
        Tags = new[] { "Ladevorgänge" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetConnectorStatus(Guid connectorId)
    {
        try
        {
            await _chargingService.ResetConnectorStatusAsync(connectorId);
            return Ok(new { message = "Connector-Status erfolgreich zurückgesetzt" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("admin/cleanup-duplicate-sessions")]
    [SwaggerOperation(
        Summary = "Doppelte aktive Sessions bereinigen",
        Description = "Findet und beendet doppelte aktive Sessions auf demselben Connector. Behält nur die älteste Session.",
        OperationId = "CleanupDuplicateSessions",
        Tags = new[] { "Admin" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupDuplicateSessions()
    {
        try
        {
            var result = await _chargingService.CleanupDuplicateSessionsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
