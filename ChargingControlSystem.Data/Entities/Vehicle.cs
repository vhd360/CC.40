using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class Vehicle
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(50)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; }

    [Required]
    public VehicleType Type { get; set; }

    [Required]
    [MaxLength(100)]
    public string Color { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// RFID-Tag für Fahrzeugidentifikation (z.B. für automatische Zuordnung bei Ladevorgängen)
    /// </summary>
    [MaxLength(100)]
    public string? RfidTag { get; set; }

    /// <summary>
    /// QR-Code für Fahrzeugidentifikation
    /// </summary>
    [MaxLength(100)]
    public string? QrCode { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeactivatedAt { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<VehicleAssignment> VehicleAssignments { get; set; } = new List<VehicleAssignment>();
    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
}

public enum VehicleType
{
    PoolVehicle,
    CompanyVehicle
}
