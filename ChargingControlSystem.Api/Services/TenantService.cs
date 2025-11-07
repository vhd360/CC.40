using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Services;

public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;
        if (tenantId.HasValue)
        {
            return await GetTenantByIdAsync(tenantId.Value);
        }
        return null;
    }

    public Guid GetCurrentTenantId()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;
        if (tenantId.HasValue)
        {
            return tenantId.Value;
        }
        
        // Fallback: Try to get from current user's claims
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = user.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var parsedTenantId))
            {
                return parsedTenantId;
            }
        }
        
        throw new InvalidOperationException("No tenant context available");
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
    {
        return await _context.Tenants.FindAsync(tenantId);
    }

    public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
    {
        return await _context.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant)
    {
        tenant.Id = Guid.NewGuid();
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.IsActive = true;

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return tenant;
    }

    public async Task UpdateTenantAsync(Tenant tenant)
    {
        tenant.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTenantAsync(Guid tenantId)
    {
        var tenant = await GetTenantByIdAsync(tenantId);
        if (tenant != null)
        {
            tenant.IsActive = false;
            await UpdateTenantAsync(tenant);
        }
    }
}
