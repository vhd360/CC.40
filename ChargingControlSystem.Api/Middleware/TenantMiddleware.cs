using ChargingControlSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Skip tenant resolution for public endpoints (Auth, Registration, User Portal)
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/api/auth") ||
            context.Request.Path.StartsWithSegments("/api/tenants/register") ||
            context.Request.Path.StartsWithSegments("/api/user-groups/join") ||
            context.Request.Path.StartsWithSegments("/api/user-portal"))
        {
            await _next(context);
            return;
        }

        // 1. Try to get TenantId from JWT Token (authenticated users)
        var tenantIdClaim = context.User?.FindFirst("TenantId")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
            
            if (tenant != null)
            {
                context.Items["Tenant"] = tenant;
                context.Items["TenantId"] = tenant.Id;
                await _next(context);
                return;
            }
        }

        // 2. Try subdomain or header (for unauthenticated requests)
        var tenantIdentifier = GetTenantIdentifier(context);
        
        if (!string.IsNullOrEmpty(tenantIdentifier))
        {
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == tenantIdentifier && t.IsActive);

            if (tenant != null)
            {
                context.Items["Tenant"] = tenant;
                context.Items["TenantId"] = tenant.Id;
                await _next(context);
                return;
            }
        }

        // 3. No tenant found
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Tenant not found");
    }

    private string? GetTenantIdentifier(HttpContext context)
    {
        // Try subdomain first
        var host = context.Request.Host.Host;
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (subdomain != "www" && subdomain != "localhost")
            {
                return subdomain;
            }
        }

        // Try header as fallback
        if (context.Request.Headers.TryGetValue("X-Tenant", out var tenantHeader))
        {
            return tenantHeader.ToString();
        }

        // For development, use default tenant
        if (context.Request.Host.Host == "localhost")
        {
            return "default";
        }

        return null;
    }
}
