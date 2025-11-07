namespace ChargingControlSystem.Data.Enums;

public enum UserRole
{
    /// <summary>
    /// System Administrator - can manage all tenants and system-wide settings
    /// </summary>
    SuperAdmin = 0,
    
    /// <summary>
    /// Tenant Administrator - can manage their own tenant and users
    /// </summary>
    TenantAdmin = 1,
    
    /// <summary>
    /// Regular User - can use the system within their tenant
    /// </summary>
    User = 2
}


