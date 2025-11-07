using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using ChargingControlSystem.Api.Authorization;
using ChargingControlSystem.Data.Enums;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TenantsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequireRole(UserRole.SuperAdmin, UserRole.TenantAdmin)]
    public async Task<IActionResult> GetAll()
    {
        var roleClaim = User.FindFirst("Role")?.Value;
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;

        IQueryable<ChargingControlSystem.Data.Entities.Tenant> query = _context.Tenants;

        // TenantAdmins can see their own tenant and all sub-tenants (recursively)
        if (roleClaim == UserRole.TenantAdmin.ToString() && !string.IsNullOrEmpty(tenantIdClaim))
        {
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                // Get all tenant IDs in the hierarchy
                var tenantIds = await GetTenantHierarchyIds(tenantId);
                query = query.Where(t => tenantIds.Contains(t.Id));
            }
        }
        // SuperAdmins can see all tenants (no filter)

        var tenants = await query
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Subdomain,
                t.Description,
                t.ParentTenantId,
                t.LogoUrl,
                Theme = (int)t.Theme,
                t.IsActive,
                t.CreatedAt,
                UserCount = t.Users.Count,
                SubTenantCount = t.SubTenants.Count
            })
            .ToListAsync();

        return Ok(tenants);
    }

    // Helper method to get all tenant IDs in a hierarchy (current + all descendants)
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

    [HttpGet("{id}")]
    [RequireRole(UserRole.SuperAdmin, UserRole.TenantAdmin)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // TenantAdmins can access their own tenant and all sub-tenants in their hierarchy
        var roleClaim = User.FindFirst("Role")?.Value;
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        
        if (roleClaim == UserRole.TenantAdmin.ToString() && !string.IsNullOrEmpty(tenantIdClaim))
        {
            if (Guid.TryParse(tenantIdClaim, out var userTenantId))
            {
                // Get all tenant IDs in the user's hierarchy (own tenant + all sub-tenants)
                var allowedTenantIds = await GetTenantHierarchyIds(userTenantId);
                
                // Check if the requested tenant is in the allowed hierarchy
                if (!allowedTenantIds.Contains(id))
                {
                    return Forbid();
                }
            }
        }

        var tenant = await _context.Tenants
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Subdomain,
                t.Description,
                t.ParentTenantId,
                ParentTenantName = t.ParentTenant != null ? t.ParentTenant.Name : null,
                t.Address,
                t.PostalCode,
                t.City,
                t.Country,
                t.Phone,
                t.Email,
                t.Website,
                t.TaxId,
                t.LogoUrl,
                Theme = (int)t.Theme,
                t.IsActive,
                t.CreatedAt,
                t.UpdatedAt,
                UserCount = t.Users.Count,
                ChargingParkCount = t.ChargingParks.Count,
                VehicleCount = t.Vehicles.Count,
                SubTenantCount = t.SubTenants.Count
            })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

    [HttpPost]
    [RequireRole(UserRole.SuperAdmin, UserRole.TenantAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var roleClaim = User.FindFirst("Role")?.Value;
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;

        // Check if subdomain already exists
        var existingTenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == dto.Subdomain.ToLower());

        if (existingTenant != null)
            return BadRequest(new { error = "Subdomain already exists", field = "subdomain" });

            Guid? parentTenantId = null;
            
            // TenantAdmins create sub-tenants under their own tenant
            if (roleClaim == UserRole.TenantAdmin.ToString() && !string.IsNullOrEmpty(tenantIdClaim))
            {
                parentTenantId = Guid.Parse(tenantIdClaim);
            }
            // SuperAdmins can optionally specify a parent tenant
            else if (roleClaim == UserRole.SuperAdmin.ToString() && dto.ParentTenantId.HasValue)
            {
                parentTenantId = dto.ParentTenantId;
            }

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Subdomain = dto.Subdomain.ToLower(),
                Description = dto.Description,
                ParentTenantId = parentTenantId,
                Address = dto.Address,
                PostalCode = dto.PostalCode,
                City = dto.City,
                Country = dto.Country,
                Phone = dto.Phone,
                Email = dto.Email,
                Website = dto.Website,
                TaxId = dto.TaxId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, new
            {
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.Description,
                tenant.ParentTenantId,
                tenant.Address,
                tenant.PostalCode,
                tenant.City,
                tenant.Country,
                tenant.Phone,
                tenant.Email,
                tenant.Website,
                tenant.TaxId,
                tenant.IsActive,
                tenant.CreatedAt
            });
    }

    [HttpPut("{id}")]
    [RequireRole(UserRole.SuperAdmin, UserRole.TenantAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto)
    {
        // TenantAdmins can only update their own tenant
        var roleClaim = User.FindFirst("Role")?.Value;
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        
        if (roleClaim == UserRole.TenantAdmin.ToString() && tenantIdClaim != id.ToString())
        {
            return Forbid();
        }

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        // Check if subdomain is being changed and if new subdomain already exists
        if (dto.Subdomain.ToLower() != tenant.Subdomain.ToLower())
        {
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == dto.Subdomain.ToLower() && t.Id != id);

            if (existingTenant != null)
                return BadRequest(new { error = "Subdomain already exists", field = "subdomain" });
        }

            tenant.Name = dto.Name;
            tenant.Subdomain = dto.Subdomain.ToLower();
            tenant.Description = dto.Description;
            tenant.Address = dto.Address;
            tenant.PostalCode = dto.PostalCode;
            tenant.City = dto.City;
            tenant.Country = dto.Country;
            tenant.Phone = dto.Phone;
            tenant.Email = dto.Email;
            tenant.Website = dto.Website;
            tenant.TaxId = dto.TaxId;
            if (dto.Theme.HasValue)
            {
                tenant.Theme = (Data.Enums.TenantTheme)dto.Theme.Value;
            }
            tenant.IsActive = dto.IsActive;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.Description,
                tenant.Address,
                tenant.PostalCode,
                tenant.City,
                tenant.Country,
                tenant.Phone,
                tenant.Email,
                tenant.Website,
                tenant.TaxId,
                tenant.IsActive,
                tenant.UpdatedAt
            });
    }

    [HttpDelete("{id}")]
    [RequireRole(UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        // Soft delete
        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterTenantDto dto)
    {
        // Check if subdomain already exists
        var existingTenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == dto.Subdomain.ToLower());

        if (existingTenant != null)
            return BadRequest(new { error = "Subdomain already exists", field = "subdomain" });

        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.AdminEmail.ToLower());

        if (existingUser != null)
            return BadRequest(new { error = "Email already exists", field = "adminEmail" });

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = dto.CompanyName,
                Subdomain = dto.Subdomain.ToLower(),
                Description = dto.Description,
                Address = dto.Address,
                PostalCode = dto.PostalCode,
                City = dto.City,
                Country = dto.Country,
                Phone = dto.Phone,
                Email = dto.Email,
                Website = dto.Website,
                TaxId = dto.TaxId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

        _context.Tenants.Add(tenant);

        // Create admin user for the tenant
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = dto.AdminEmail.ToLower(),
            FirstName = dto.AdminFirstName,
            LastName = dto.AdminLastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword),
            Role = Data.Enums.UserRole.TenantAdmin,
            IsActive = true,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, new
        {
            tenant.Id,
            tenant.Name,
            tenant.Subdomain,
            AdminUserId = adminUser.Id,
            Message = "Tenant registered successfully. Please verify your email."
        });
    }
}

public record CreateTenantDto(
    string Name,
    string Subdomain,
    string? Description,
    Guid? ParentTenantId,
    string? Address,
    string? PostalCode,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string? Website,
    string? TaxId
);

public record UpdateTenantDto(
    string Name,
    string Subdomain,
    string? Description,
    string? Address,
    string? PostalCode,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string? Website,
    string? TaxId,
    int? Theme,
    bool IsActive
);

public record RegisterTenantDto(
    string CompanyName,
    string Subdomain,
    string? Description,
    string? Address,
    string? PostalCode,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string? Website,
    string? TaxId,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    string AdminPassword
);
