using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class BillingTransaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BillingAccountId { get; set; }

    public Guid? ChargingSessionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TransactionType { get; set; } = string.Empty; // "charging", "subscription", "payment"

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    [MaxLength(255)]
    public string? StripePaymentIntentId { get; set; }

    [Required]
    public BillingTransactionStatus Status { get; set; } = BillingTransactionStatus.Pending;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    public virtual BillingAccount BillingAccount { get; set; } = null!;
    public virtual ChargingSession? ChargingSession { get; set; }
}

public enum BillingTransactionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    Cancelled
}
