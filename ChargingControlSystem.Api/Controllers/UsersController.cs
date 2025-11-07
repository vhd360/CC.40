using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using ChargingControlSystem.Data.Enums;
using ChargingControlSystem.Api.Authorization;
using BCrypt.Net;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequireRole(UserRole.SuperAdmin, UserRole.TenantAdmin)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId = null)
    {
        var currentTenantId = tenantId ?? HttpContext.Items["TenantId"] as Guid?;
        if (currentTenantId == null)
            return BadRequest("Tenant not found");

        // Get "own" users (users belonging to this tenant)
        var ownUsers = await _context.Users
            .Where(u => u.TenantId == currentTenantId.Value)
            .Select(u => new
            {
                u.Id,
                u.TenantId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                Role = u.Role.ToString(),
                u.IsActive,
                u.IsEmailConfirmed,
                u.CreatedAt,
                u.LastLoginAt,
                IsGuest = false,
                GroupMemberships = new List<string>()
            })
            .ToListAsync();

        // Get "guest" users (users from other tenants who are members of this tenant's user groups)
        var guestUsers = await _context.UserGroupMemberships
            .Include(m => m.User)
            .Include(m => m.UserGroup)
            .Where(m => m.UserGroup.TenantId == currentTenantId.Value && m.User.TenantId != currentTenantId.Value)
            .GroupBy(m => m.User)
            .Select(g => new
            {
                g.Key.Id,
                g.Key.TenantId,
                g.Key.FirstName,
                g.Key.LastName,
                g.Key.Email,
                g.Key.PhoneNumber,
                Role = g.Key.Role.ToString(),
                g.Key.IsActive,
                g.Key.IsEmailConfirmed,
                g.Key.CreatedAt,
                g.Key.LastLoginAt,
                IsGuest = true,
                GroupMemberships = g.Select(m => m.UserGroup.Name).ToList()
            })
            .ToListAsync();

        // Combine both lists
        var allUsers = ownUsers.Concat(guestUsers).OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

        return Ok(allUsers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.IsActive,
                u.IsEmailConfirmed,
                u.CreatedAt,
                u.LastLoginAt,
                VehicleAssignments = u.VehicleAssignments.Count,
                ChargingSessions = u.ChargingSessions.Count
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    [RequireRole(UserRole.SuperAdmin, UserRole.TenantAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        // Get current user's role and tenant
        var roleClaim = User.FindFirst("Role")?.Value;
        var userTenantIdClaim = User.FindFirst("TenantId")?.Value;

        // Determine which tenant to create the user for
        var targetTenantId = dto.TenantId;

        // Authorization checks
        if (roleClaim == UserRole.TenantAdmin.ToString())
        {
            if (!Guid.TryParse(userTenantIdClaim, out var userTenantId))
                return Unauthorized("Invalid tenant");

            // TenantAdmins can only create users for their own tenant or sub-tenants
            var allowedTenantIds = await GetTenantHierarchyIds(userTenantId);

            if (!allowedTenantIds.Contains(targetTenantId))
                return Forbid("You can only create users for your own tenant or sub-tenants");

            // TenantAdmins cannot create SuperAdmins
            if (dto.Role == UserRole.SuperAdmin)
                return Forbid("You cannot create SuperAdmin users");
        }

        // Check if target tenant exists and is active
        var targetTenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == targetTenantId && t.IsActive);
        
        if (targetTenant == null)
            return BadRequest("Target tenant not found or inactive");

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            return BadRequest(new { error = "Email already exists", field = "email" });

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = targetTenantId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.ToLower(),
            PhoneNumber = dto.PhoneNumber,
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new
        {
            user.Id,
            user.TenantId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            Role = user.Role.ToString(),
            user.IsActive
        });
    }

    // Helper method to get all tenant IDs in a hierarchy (own tenant + all sub-tenants recursively)
    private async Task<List<Guid>> GetTenantHierarchyIds(Guid rootTenantId)
    {
        var allIds = new List<Guid> { rootTenantId };
        var toProcess = new Queue<Guid>();
        toProcess.Enqueue(rootTenantId);

        while (toProcess.Count > 0)
        {
            var currentId = toProcess.Dequeue();
            var childIds = await _context.Tenants
                .Where(t => t.ParentTenantId == currentId)
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var childId in childIds)
            {
                if (!allIds.Contains(childId))
                {
                    allIds.Add(childId);
                    toProcess.Enqueue(childId);
                }
            }
        }

        return allIds;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.IsActive = dto.IsActive;

        // Update password if provided
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.IsActive
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}/remove-from-tenant")]
    [RequireRole(UserRole.SuperAdmin, UserRole.TenantAdmin)]
    public async Task<IActionResult> RemoveGuestUserFromTenant(Guid id)
    {
        var currentTenantId = HttpContext.Items["TenantId"] as Guid?;
        if (currentTenantId == null)
            return BadRequest("Tenant not found");

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found");

        // Prevent removing users that belong to this tenant
        if (user.TenantId == currentTenantId.Value)
            return BadRequest("Cannot remove own tenant users using this endpoint. Use the regular delete instead.");

        // Find all memberships in this tenant's user groups
        var memberships = await _context.UserGroupMemberships
            .Include(m => m.UserGroup)
            .Where(m => m.UserId == id && m.UserGroup.TenantId == currentTenantId.Value)
            .ToListAsync();

        if (memberships.Count == 0)
            return NotFound("User is not a member of any groups in this tenant");

        // Remove all memberships
        _context.UserGroupMemberships.RemoveRange(memberships);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "User removed from all tenant groups",
            RemovedMemberships = memberships.Count,
            Groups = memberships.Select(m => m.UserGroup.Name).ToList()
        });
    }
}

public record CreateUserDto(
    Guid TenantId, 
    string FirstName, 
    string LastName, 
    string Email, 
    string? PhoneNumber, 
    string Password,
    UserRole Role
);
public record UpdateUserDto(string FirstName, string LastName, string Email, string? PhoneNumber, bool IsActive, string? Password);

