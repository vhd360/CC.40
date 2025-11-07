using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Services;

public interface ITenantService
{
    Task<Tenant?> GetCurrentTenantAsync();
    Guid GetCurrentTenantId();
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId);
    Task<Tenant?> GetTenantBySubdomainAsync(string subdomain);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync();
    Task<Tenant> CreateTenantAsync(Tenant tenant);
    Task UpdateTenantAsync(Tenant tenant);
    Task DeleteTenantAsync(Guid tenantId);
}
