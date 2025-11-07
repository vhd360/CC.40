using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/charging-parks")]
public class ChargingParksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChargingParksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var parks = await _context.ChargingParks
            .Where(p => p.TenantId == tenantId.Value && p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Address,
                p.PostalCode,
                p.City,
                p.Country,
                p.Latitude,
                p.Longitude,
                p.IsActive,
                p.CreatedAt,
                StationCount = p.ChargingStations.Count
            })
            .ToListAsync();

        return Ok(parks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var park = await _context.ChargingParks
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Address,
                p.PostalCode,
                p.City,
                p.Country,
                p.Latitude,
                p.Longitude,
                p.IsActive,
                p.CreatedAt,
                Stations = p.ChargingStations.Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.StationId,
                    Status = s.Status.ToString()
                })
            })
            .FirstOrDefaultAsync();

        if (park == null)
            return NotFound();

        return Ok(park);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChargingParkDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var park = new ChargingControlSystem.Data.Entities.ChargingPark
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Name = dto.Name,
            Description = dto.Description,
            Address = dto.Address,
            PostalCode = dto.PostalCode,
            City = dto.City,
            Country = dto.Country,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChargingParks.Add(park);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = park.Id }, new
        {
            park.Id,
            park.Name,
            park.Description,
            park.Address,
            park.PostalCode,
            park.City,
            park.Country,
            park.Latitude,
            park.Longitude,
            park.IsActive,
            park.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChargingParkDto dto)
    {
        var park = await _context.ChargingParks.FindAsync(id);
        if (park == null)
            return NotFound();

        park.Name = dto.Name;
        park.Description = dto.Description;
        park.Address = dto.Address;
        park.PostalCode = dto.PostalCode;
        park.City = dto.City;
        park.Country = dto.Country;
        park.Latitude = dto.Latitude;
        park.Longitude = dto.Longitude;
        park.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            park.Id,
            park.Name,
            park.Description,
            park.Address,
            park.PostalCode,
            park.City,
            park.Country,
            park.Latitude,
            park.Longitude,
            park.IsActive,
            park.CreatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var park = await _context.ChargingParks.FindAsync(id);
        if (park == null)
            return NotFound();

        park.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateChargingParkDto(
    string Name,
    string? Description,
    string Address,
    string PostalCode,
    string City,
    string Country,
    decimal? Latitude,
    decimal? Longitude
);

public record UpdateChargingParkDto(
    string Name,
    string? Description,
    string Address,
    string PostalCode,
    string City,
    string Country,
    decimal? Latitude,
    decimal? Longitude,
    bool IsActive
);

