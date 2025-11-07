using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class BillingAccount
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string AccountName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? StripeCustomerId { get; set; }

    [Required]
    public BillingAccountType Type { get; set; }

    [Required]
    public BillingAccountStatus Status { get; set; } = BillingAccountStatus.Active;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeactivatedAt { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<BillingTransaction> Transactions { get; set; } = new List<BillingTransaction>();
}

public enum BillingAccountType
{
    Individual,
    Company,
    PoolAccount
}

public enum BillingAccountStatus
{
    Active,
    Suspended,
    Closed
}
