using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var stats = new
        {
            TotalStations = await _context.ChargingStations
                .Include(s => s.ChargingPark)
                .CountAsync(s => s.ChargingPark.TenantId == tenantId.Value),
            TotalVehicles = await _context.Vehicles
                .CountAsync(v => v.TenantId == tenantId.Value),
            TotalTransactions = await _context.BillingTransactions
                .Include(t => t.BillingAccount)
                .CountAsync(t => t.BillingAccount.TenantId == tenantId.Value),
            ActiveStations = await _context.ChargingStations
                .Include(s => s.ChargingPark)
                .CountAsync(s => s.ChargingPark.TenantId == tenantId.Value && s.IsActive && s.Status == ChargingStationStatus.Available),
            ActiveVehicles = await _context.Vehicles
                .CountAsync(v => v.TenantId == tenantId.Value && v.IsActive)
        };

        return Ok(stats);
    }
}

