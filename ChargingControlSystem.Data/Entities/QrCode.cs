using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class QrCode
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty; // Unique QR code string

    [Required]
    public QrCodeType Type { get; set; }

    public Guid? ChargingParkId { get; set; } // für Park-Einladungen

    public Guid? UserId { get; set; } // für persönliche QR-Codes

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int? MaxUses { get; set; }

    public int CurrentUses { get; set; } = 0;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ChargingPark? ChargingPark { get; set; }
    public virtual User? User { get; set; }

    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
}

public enum QrCodeType
{
    ParkInvitation,    // Einladung für einen Ladepark
    AdHocCharging,     // Adhoc-Ladung
    PersonalCode       // Persönlicher QR-Code eines Benutzers
}
