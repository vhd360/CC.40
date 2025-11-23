using ChargingControlSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DebugController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("station/{chargeBoxId}")]
    public async Task<IActionResult> GetStationDebug(string chargeBoxId)
    {
        var station = await _context.ChargingStations
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station == null)
            return NotFound();

        return Ok(new
        {
            station.Id,
            station.StationId,
            station.Name,
            station.ChargeBoxId,
            station.Status,
            station.LastHeartbeat,
            LastHeartbeatRaw = station.LastHeartbeat?.ToString("o"),
            LastHeartbeatKind = station.LastHeartbeat?.Kind.ToString(),
            NowUtc = DateTime.UtcNow,
            NowLocal = DateTime.Now,
            MinutesSince = station.LastHeartbeat.HasValue 
                ? (DateTime.UtcNow - station.LastHeartbeat.Value).TotalMinutes 
                : (double?)null
        });
    }
}

