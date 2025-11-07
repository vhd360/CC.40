using System.ComponentModel.DataAnnotations;

namespace ChargingControlSystem.Data.Entities;

public enum AuthorizationMethodType
{
    RFID,
    Autocharge,
    App,
    QRCode,
    CreditCard,
    PlugAndCharge
}

public class AuthorizationMethod
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public AuthorizationMethodType Type { get; set; }

    [Required]
    [MaxLength(200)]
    public string Identifier { get; set; } = string.Empty; // RFID-Tag-Nummer, VIN, etc.

    [MaxLength(100)]
    public string? FriendlyName { get; set; } // z.B. "Meine RFID-Karte"

    [Required]
    public bool IsActive { get; set; } = true;

    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidUntil { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }

    // Zusätzliche Metadaten als JSON
    [MaxLength(1000)]
    public string? Metadata { get; set; } // z.B. {"vehicleVIN": "...", "manufacturer": "..."}

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
}

