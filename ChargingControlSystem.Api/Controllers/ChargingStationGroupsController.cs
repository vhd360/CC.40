using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/charging-station-groups")]
public class ChargingStationGroupsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChargingStationGroupsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var groups = await _context.ChargingStationGroups
            .Where(g => g.TenantId == tenantId.Value && g.IsActive)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                g.IsActive,
                g.CreatedAt,
                StationCount = g.StationMemberships.Count
            })
            .ToListAsync();

        return Ok(groups);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var group = await _context.ChargingStationGroups
            .Where(g => g.Id == id)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                g.IsActive,
                g.CreatedAt,
                Stations = g.StationMemberships.Select(m => new
                {
                    m.ChargingStationId,
                    StationName = m.ChargingStation.Name,
                    StationId = m.ChargingStation.StationId,
                    Status = m.ChargingStation.Status.ToString(),
                    m.AssignedAt
                })
            })
            .FirstOrDefaultAsync();

        if (group == null)
            return NotFound();

        return Ok(group);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChargingStationGroupDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var group = new ChargingStationGroup
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChargingStationGroups.Add(group);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChargingStationGroupDto dto)
    {
        var group = await _context.ChargingStationGroups.FindAsync(id);
        if (group == null)
            return NotFound();

        group.Name = dto.Name;
        group.Description = dto.Description;
        group.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(group);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var group = await _context.ChargingStationGroups.FindAsync(id);
        if (group == null)
            return NotFound();

        group.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/stations/{stationId}")]
    public async Task<IActionResult> AddStation(Guid id, Guid stationId)
    {
        var group = await _context.ChargingStationGroups.FindAsync(id);
        if (group == null)
            return NotFound("Group not found");

        var station = await _context.ChargingStations.FindAsync(stationId);
        if (station == null)
            return NotFound("Station not found");

        // Check if already exists
        var exists = await _context.ChargingStationGroupMemberships
            .AnyAsync(m => m.ChargingStationGroupId == id && m.ChargingStationId == stationId);

        if (exists)
            return BadRequest("Station already in group");

        var membership = new ChargingStationGroupMembership
        {
            Id = Guid.NewGuid(),
            ChargingStationGroupId = id,
            ChargingStationId = stationId,
            AssignedAt = DateTime.UtcNow
        };

        _context.ChargingStationGroupMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            membership.Id, 
            membership.ChargingStationGroupId, 
            membership.ChargingStationId, 
            membership.AssignedAt 
        });
    }

    [HttpDelete("{id}/stations/{stationId}")]
    public async Task<IActionResult> RemoveStation(Guid id, Guid stationId)
    {
        var membership = await _context.ChargingStationGroupMemberships
            .FirstOrDefaultAsync(m => m.ChargingStationGroupId == id && m.ChargingStationId == stationId);

        if (membership == null)
            return NotFound();

        _context.ChargingStationGroupMemberships.Remove(membership);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateChargingStationGroupDto(string Name, string? Description);
public record UpdateChargingStationGroupDto(string Name, string? Description, bool IsActive);

