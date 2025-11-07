using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChargingControlSystem.Data.Enums;

namespace ChargingControlSystem.Data.Entities;

public class Tenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Subdomain { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Hierarchical structure
    public Guid? ParentTenantId { get; set; }
    
    [ForeignKey("ParentTenantId")]
    public virtual Tenant? ParentTenant { get; set; }
    
    public virtual ICollection<Tenant> SubTenants { get; set; } = new List<Tenant>();

    // Contact Information
    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; } // Steuernummer / USt-IdNr.

    // Branding
    [MaxLength(500)]
    public string? LogoUrl { get; set; } // Pfad zum Logo (z.B. /uploads/tenants/{tenantId}/logo.png)

    [Required]
    public TenantTheme Theme { get; set; } = TenantTheme.Blue; // Farbschema

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<ChargingPark> ChargingParks { get; set; } = new List<ChargingPark>();
    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
    public virtual ICollection<BillingAccount> BillingAccounts { get; set; } = new List<BillingAccount>();
}
