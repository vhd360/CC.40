using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/user-groups")]
public class UserGroupsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserGroupsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var groups = await _context.UserGroups
            .Where(g => g.TenantId == tenantId.Value && g.IsActive)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                g.IsActive,
                g.CreatedAt,
                MemberCount = g.UserGroupMemberships.Count,
                PermissionCount = g.GroupPermissions.Count
            })
            .ToListAsync();

        return Ok(groups);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var group = await _context.UserGroups
            .Where(g => g.Id == id)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                g.IsActive,
                g.CreatedAt,
                Members = g.UserGroupMemberships.Select(m => new
                {
                    m.UserId,
                    UserName = $"{m.User.FirstName} {m.User.LastName}",
                    m.User.Email,
                    m.AssignedAt
                }),
                Permissions = g.GroupPermissions.Select(p => new
                {
                    p.PermissionId,
                    p.Permission.Name,
                    p.Permission.Description,
                    p.AssignedAt
                })
            })
            .FirstOrDefaultAsync();

        if (group == null)
            return NotFound();

        return Ok(group);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserGroupDto dto)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var group = new UserGroup
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserGroups.Add(group);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserGroupDto dto)
    {
        var group = await _context.UserGroups.FindAsync(id);
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
        var group = await _context.UserGroups.FindAsync(id);
        if (group == null)
            return NotFound();

        group.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/users/{userId}")]
    public async Task<IActionResult> AddUser(Guid id, Guid userId)
    {
        var group = await _context.UserGroups.FindAsync(id);
        if (group == null)
            return NotFound("Group not found");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User not found");

        // Check if already exists
        var exists = await _context.UserGroupMemberships
            .AnyAsync(m => m.UserGroupId == id && m.UserId == userId);

        if (exists)
            return BadRequest("User already in group");

        var membership = new UserGroupMembership
        {
            Id = Guid.NewGuid(),
            UserGroupId = id,
            UserId = userId,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserGroupMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            membership.Id,
            membership.UserGroupId,
            membership.UserId,
            membership.AssignedAt
        });
    }

    [HttpDelete("{id}/users/{userId}")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId)
    {
        var membership = await _context.UserGroupMemberships
            .FirstOrDefaultAsync(m => m.UserGroupId == id && m.UserId == userId);

        if (membership == null)
            return NotFound();

        _context.UserGroupMemberships.Remove(membership);
        await _context.SaveChangesAsync();

        return NoContent();
    }

        [HttpPost("{id}/generate-invite")]
        public async Task<IActionResult> GenerateInvite(Guid id, [FromBody] GenerateInviteDto? dto = null)
        {
            var group = await _context.UserGroups.FindAsync(id);
            if (group == null)
                return NotFound("Group not found");

            // Generate a unique token (combine two GUIDs without dashes)
            var token = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
            
            // Set expiration (default 7 days, or from dto)
            var expiresAt = DateTime.UtcNow.AddDays(dto?.ExpiryDays ?? 7);

            group.InviteToken = token;
            group.InviteTokenExpiresAt = expiresAt;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Token = token,
                ExpiresAt = expiresAt,
                InviteUrl = $"/join-group?token={token}"
            });
        }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGroupWithToken([FromBody] JoinGroupDto dto)
    {
        // Find group by invite token
        var group = await _context.UserGroups
            .FirstOrDefaultAsync(g => g.InviteToken == dto.Token && g.IsActive);

        if (group == null)
            return NotFound("Invalid invite token");

        // Check if token is expired
        if (group.InviteTokenExpiresAt.HasValue && group.InviteTokenExpiresAt.Value < DateTime.UtcNow)
            return BadRequest("Invite token has expired");

        // Get current user ID from JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("User not authenticated");

        // Check if already a member
        var exists = await _context.UserGroupMemberships
            .AnyAsync(m => m.UserGroupId == group.Id && m.UserId == userId);

        if (exists)
            return BadRequest("You are already a member of this group");

        // Add user to group
        var membership = new UserGroupMembership
        {
            Id = Guid.NewGuid(),
            UserGroupId = group.Id,
            UserId = userId,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserGroupMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Successfully joined group",
            GroupId = group.Id,
            GroupName = group.Name
        });
    }

    [HttpDelete("{id}/revoke-invite")]
    public async Task<IActionResult> RevokeInvite(Guid id)
    {
        var group = await _context.UserGroups.FindAsync(id);
        if (group == null)
            return NotFound("Group not found");

        group.InviteToken = null;
        group.InviteTokenExpiresAt = null;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Invite token revoked" });
    }

    // Charging Station Group Permissions

    [HttpGet("{id}/station-permissions")]
    public async Task<IActionResult> GetStationPermissions(Guid id)
    {
        var permissions = await _context.UserGroupChargingStationGroupPermissions
            .Include(p => p.ChargingStationGroup)
            .Where(p => p.UserGroupId == id)
            .Select(p => new
            {
                p.Id,
                p.ChargingStationGroupId,
                ChargingStationGroupName = p.ChargingStationGroup.Name,
                AssignedAt = p.GrantedAt
            })
            .ToListAsync();

        return Ok(permissions);
    }

    [HttpPost("{id}/station-permissions")]
    public async Task<IActionResult> AddStationPermission(Guid id, [FromBody] AddStationPermissionDto dto)
    {
        var group = await _context.UserGroups.FindAsync(id);
        if (group == null)
            return NotFound("Group not found");

        var stationGroup = await _context.ChargingStationGroups.FindAsync(dto.ChargingStationGroupId);
        if (stationGroup == null)
            return NotFound("Charging station group not found");

        // Check if permission already exists
        var exists = await _context.UserGroupChargingStationGroupPermissions
            .AnyAsync(p => p.UserGroupId == id && p.ChargingStationGroupId == dto.ChargingStationGroupId);

        if (exists)
            return BadRequest("Permission already exists");

        var permission = new UserGroupChargingStationGroupPermission
        {
            Id = Guid.NewGuid(),
            UserGroupId = id,
            ChargingStationGroupId = dto.ChargingStationGroupId,
            GrantedAt = DateTime.UtcNow
        };

        _context.UserGroupChargingStationGroupPermissions.Add(permission);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            permission.Id,
            permission.UserGroupId,
            permission.ChargingStationGroupId,
            ChargingStationGroupName = stationGroup.Name,
            AssignedAt = permission.GrantedAt
        });
    }

    [HttpDelete("{id}/station-permissions/{chargingStationGroupId}")]
    public async Task<IActionResult> RemoveStationPermission(Guid id, Guid chargingStationGroupId)
    {
        var permission = await _context.UserGroupChargingStationGroupPermissions
            .FirstOrDefaultAsync(p => p.UserGroupId == id && p.ChargingStationGroupId == chargingStationGroupId);

        if (permission == null)
            return NotFound("Permission not found");

        _context.UserGroupChargingStationGroupPermissions.Remove(permission);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateUserGroupDto(string Name, string? Description);
public record UpdateUserGroupDto(string Name, string? Description, bool IsActive);
public record GenerateInviteDto(int ExpiryDays = 7);
public record JoinGroupDto(string Token);
public record AddStationPermissionDto(Guid ChargingStationGroupId);

