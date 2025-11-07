using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

/// <summary>
/// Represents a pricing tariff that can be assigned to user groups
/// Flexible pricing model supporting kWh-based, time-based, and combined pricing
/// </summary>
public class Tariff
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Currency code (ISO 4217), e.g., EUR, USD
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Is this the default tariff for the tenant?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Tariff is active and can be used
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Valid from date (optional)
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Valid until date (optional)
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<TariffComponent> Components { get; set; } = new List<TariffComponent>();
    public virtual ICollection<UserGroupTariff> UserGroupTariffs { get; set; } = new List<UserGroupTariff>();
}

/// <summary>
/// Individual pricing components of a tariff
/// A tariff can have multiple components (e.g., energy + parking time)
/// </summary>
public class TariffComponent
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TariffId { get; set; }

    /// <summary>
    /// Type of pricing component
    /// </summary>
    [Required]
    public TariffComponentType Type { get; set; }

    /// <summary>
    /// Price per unit (e.g., per kWh, per minute)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 4)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Step size for stepped pricing (optional)
    /// E.g., bill every 1 kWh or every 15 minutes
    /// </summary>
    public int? StepSize { get; set; }

    /// <summary>
    /// Start time for time-of-day pricing (HH:mm format, UTC)
    /// </summary>
    [MaxLength(5)]
    public string? TimeStart { get; set; }

    /// <summary>
    /// End time for time-of-day pricing (HH:mm format, UTC)
    /// </summary>
    [MaxLength(5)]
    public string? TimeEnd { get; set; }

    /// <summary>
    /// Days of week this component applies (comma-separated, 0=Sunday, 6=Saturday)
    /// E.g., "1,2,3,4,5" for weekdays only
    /// </summary>
    [MaxLength(50)]
    public string? DaysOfWeek { get; set; }

    /// <summary>
    /// Minimum charge to apply (e.g., minimum energy to start billing)
    /// </summary>
    [Column(TypeName = "decimal(10, 4)")]
    public decimal? MinimumCharge { get; set; }

    /// <summary>
    /// Maximum charge cap (e.g., maximum parking fee)
    /// </summary>
    [Column(TypeName = "decimal(10, 4)")]
    public decimal? MaximumCharge { get; set; }

    /// <summary>
    /// Grace period in minutes before this component starts billing
    /// E.g., first 2 hours of parking free
    /// </summary>
    public int? GracePeriodMinutes { get; set; }

    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tariff Tariff { get; set; } = null!;
}

/// <summary>
/// Links tariffs to user groups
/// Each user group can have one tariff assigned
/// </summary>
public class UserGroupTariff
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserGroupId { get; set; }

    [Required]
    public Guid TariffId { get; set; }

    /// <summary>
    /// Priority if multiple tariffs could apply (higher = higher priority)
    /// </summary>
    public int Priority { get; set; } = 0;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual UserGroup UserGroup { get; set; } = null!;
    public virtual Tariff Tariff { get; set; } = null!;
}

/// <summary>
/// Types of tariff components
/// </summary>
public enum TariffComponentType
{
    /// <summary>
    /// Price per kWh of energy consumed
    /// </summary>
    Energy = 0,

    /// <summary>
    /// Price per minute of charging time
    /// </summary>
    ChargingTime = 1,

    /// <summary>
    /// Price per minute of parking time (including charging and after)
    /// </summary>
    ParkingTime = 2,

    /// <summary>
    /// Flat fee per charging session
    /// </summary>
    SessionFee = 3,

    /// <summary>
    /// Price per minute of idle time (after charging completed)
    /// </summary>
    IdleTime = 4,

    /// <summary>
    /// Price based on time of day (peak/off-peak)
    /// </summary>
    TimeOfDay = 5
}

