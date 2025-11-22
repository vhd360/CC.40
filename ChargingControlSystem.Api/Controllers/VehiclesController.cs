using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VehiclesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var vehicles = await _context.Vehicles
            .Where(v => v.TenantId == tenantId.Value)
            .Select(v => new
            {
                v.Id,
                v.LicensePlate,
                v.Make,
                v.Model,
                v.Year,
                Type = v.Type.ToString(),
                v.Color,
                v.Notes,
                v.RfidTag,
                v.QrCode,
                v.IsActive,
                v.CreatedAt
            })
            .ToListAsync();

        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var vehicle = await _context.Vehicles
            .Where(v => v.Id == id && v.TenantId == tenantId.Value)
            .Select(v => new
            {
                v.Id,
                v.LicensePlate,
                v.Make,
                v.Model,
                v.Year,
                Type = v.Type.ToString(),
                v.Color,
                v.Notes,
                v.RfidTag,
                v.QrCode,
                v.IsActive,
                v.CreatedAt,
                AssignmentCount = v.VehicleAssignments.Count,
                SessionCount = v.ChargingSessions.Count
            })
            .FirstOrDefaultAsync();

        if (vehicle == null)
            return NotFound();

        return Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var vehicle = new ChargingControlSystem.Data.Entities.Vehicle
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            LicensePlate = dto.LicensePlate,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            Type = Enum.Parse<ChargingControlSystem.Data.Entities.VehicleType>(dto.Type),
            Color = dto.Color,
            Notes = dto.Notes,
            RfidTag = dto.RfidTag,
            QrCode = dto.QrCode,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId.Value);
        if (vehicle == null)
            return NotFound();

        vehicle.LicensePlate = dto.LicensePlate;
        vehicle.Make = dto.Make;
        vehicle.Model = dto.Model;
        vehicle.Year = dto.Year;
        vehicle.Type = Enum.Parse<ChargingControlSystem.Data.Entities.VehicleType>(dto.Type);
        vehicle.Color = dto.Color;
        vehicle.Notes = dto.Notes;
        vehicle.RfidTag = dto.RfidTag;
        vehicle.QrCode = dto.QrCode;
        vehicle.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(vehicle);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId.Value);
        if (vehicle == null)
            return NotFound();

        vehicle.IsActive = false; // Soft delete
        vehicle.DeactivatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateVehicleDto(string LicensePlate, string Make, string Model, int Year, string Type, string Color, string? Notes, string? RfidTag, string? QrCode);
public record UpdateVehicleDto(string LicensePlate, string Make, string Model, int Year, string Type, string Color, string? Notes, string? RfidTag, string? QrCode, bool IsActive);

