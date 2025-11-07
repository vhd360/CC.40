using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PermissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Alle verfügbaren Berechtigungen abrufen
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _context.Permissions
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Resource,
                p.Action,
                p.Description
            })
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync();

        return Ok(permissions);
    }

    /// <summary>
    /// Berechtigungen für eine UserGroup abrufen
    /// </summary>
    [HttpGet("user-group/{userGroupId}")]
    public async Task<IActionResult> GetByUserGroup(Guid userGroupId)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        // Prüfen, ob die UserGroup zum Tenant gehört
        var userGroup = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.Id == userGroupId && ug.TenantId == tenantId.Value);

        if (userGroup == null)
            return NotFound("User group not found");

        var permissions = await _context.GroupPermissions
            .Where(gp => gp.UserGroupId == userGroupId)
            .Select(gp => new
            {
                gp.Id,
                gp.PermissionId,
                Permission = new
                {
                    gp.Permission.Id,
                    gp.Permission.Name,
                    gp.Permission.Resource,
                    gp.Permission.Action,
                    gp.Permission.Description
                },
                gp.AssignedAt
            })
            .OrderBy(gp => gp.Permission.Resource)
            .ThenBy(gp => gp.Permission.Action)
            .ToListAsync();

        return Ok(permissions);
    }

    /// <summary>
    /// Berechtigung zu UserGroup hinzufügen
    /// </summary>
    [HttpPost("user-group/{userGroupId}/permissions/{permissionId}")]
    public async Task<IActionResult> AddPermissionToGroup(Guid userGroupId, Guid permissionId)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        // Prüfen, ob die UserGroup zum Tenant gehört
        var userGroup = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.Id == userGroupId && ug.TenantId == tenantId.Value);

        if (userGroup == null)
            return NotFound("User group not found");

        // Prüfen, ob die Permission existiert
        var permission = await _context.Permissions.FindAsync(permissionId);
        if (permission == null)
            return NotFound("Permission not found");

        // Prüfen, ob die Berechtigung bereits zugewiesen ist
        var exists = await _context.GroupPermissions
            .AnyAsync(gp => gp.UserGroupId == userGroupId && gp.PermissionId == permissionId);

        if (exists)
            return BadRequest("Permission already assigned to this group");

        // Berechtigung hinzufügen
        var groupPermission = new ChargingControlSystem.Data.Entities.GroupPermission
        {
            Id = Guid.NewGuid(),
            UserGroupId = userGroupId,
            PermissionId = permissionId,
            AssignedAt = DateTime.UtcNow
        };

        _context.GroupPermissions.Add(groupPermission);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = groupPermission.Id,
            permissionId = groupPermission.PermissionId,
            permission = new
            {
                permission.Id,
                permission.Name,
                permission.Resource,
                permission.Action,
                permission.Description
            },
            assignedAt = groupPermission.AssignedAt,
            message = "Permission assigned successfully"
        });
    }

    /// <summary>
    /// Berechtigung von UserGroup entfernen
    /// </summary>
    [HttpDelete("user-group/{userGroupId}/permissions/{permissionId}")]
    public async Task<IActionResult> RemovePermissionFromGroup(Guid userGroupId, Guid permissionId)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        // Prüfen, ob die UserGroup zum Tenant gehört
        var userGroup = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.Id == userGroupId && ug.TenantId == tenantId.Value);

        if (userGroup == null)
            return NotFound("User group not found");

        // Berechtigung finden und entfernen
        var groupPermission = await _context.GroupPermissions
            .FirstOrDefaultAsync(gp => gp.UserGroupId == userGroupId && gp.PermissionId == permissionId);

        if (groupPermission == null)
            return NotFound("Permission not assigned to this group");

        _context.GroupPermissions.Remove(groupPermission);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Permission removed successfully" });
    }

    /// <summary>
    /// Alle Berechtigungen einer UserGroup auf einmal setzen
    /// </summary>
    [HttpPut("user-group/{userGroupId}/permissions")]
    public async Task<IActionResult> SetGroupPermissions(Guid userGroupId, [FromBody] SetPermissionsRequest request)
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        // Prüfen, ob die UserGroup zum Tenant gehört
        var userGroup = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.Id == userGroupId && ug.TenantId == tenantId.Value);

        if (userGroup == null)
            return NotFound("User group not found");

        // Alle bestehenden Berechtigungen entfernen
        var existingPermissions = await _context.GroupPermissions
            .Where(gp => gp.UserGroupId == userGroupId)
            .ToListAsync();

        _context.GroupPermissions.RemoveRange(existingPermissions);

        // Neue Berechtigungen hinzufügen
        foreach (var permissionId in request.PermissionIds)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission != null)
            {
                _context.GroupPermissions.Add(new ChargingControlSystem.Data.Entities.GroupPermission
                {
                    Id = Guid.NewGuid(),
                    UserGroupId = userGroupId,
                    PermissionId = permissionId,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"{request.PermissionIds.Count} permissions assigned successfully" });
    }
}

public record SetPermissionsRequest(List<Guid> PermissionIds);



