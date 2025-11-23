using ChargingControlSystem.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChargingControlSystem.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole[] _allowedRoles;

    public RequireRoleAttribute(params UserRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // First check if user is authenticated
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Try to find the Role claim - check both standard and custom claim types
        var roleClaim = context.HttpContext.User?.FindFirst("Role")?.Value 
                     ?? context.HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(roleClaim))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!Enum.TryParse<UserRole>(roleClaim, out var userRole))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!_allowedRoles.Contains(userRole))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}


