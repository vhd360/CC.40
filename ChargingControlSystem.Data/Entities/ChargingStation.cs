using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class ChargingStation
{
    [Key]
    public Guid Id { get; set; }

    // ChargingParkId ist optional für private Stationen
    public Guid? ChargingParkId { get; set; }

    // Private Station Flag
    [Required]
    public bool IsPrivate { get; set; } = false;

    // Owner für private Stationen
    public Guid? OwnerId { get; set; }

    [Required]
    [MaxLength(50)]
    public string StationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Vendor { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public ChargingStationType Type { get; set; }

    [Required]
    public int MaxPower { get; set; } // in kW

    [Required]
    public int NumberOfConnectors { get; set; }

    [Required]
    public ChargingStationStatus Status { get; set; } = ChargingStationStatus.Available;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // OCPP Configuration
    [MaxLength(100)]
    public string? ChargeBoxId { get; set; } // Eindeutige OCPP-ID (z.B. "CP001")

    [MaxLength(255)]
    public string? OcppPassword { get; set; } // Passwort für OCPP-Authentifizierung

    [MaxLength(50)]
    public string? OcppProtocol { get; set; } // z.B. "OCPP16", "OCPP201"

    [MaxLength(500)]
    public string? OcppEndpoint { get; set; } // URL zum OCPP-Server

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastHeartbeat { get; set; }

    // Navigation properties
    public virtual ChargingPark? ChargingPark { get; set; }
    public virtual User? Owner { get; set; }
    public virtual ICollection<ChargingPoint> ChargingPoints { get; set; } = new List<ChargingPoint>();
}

public enum ChargingStationType
{
    AC,
    DC
}

public enum ChargingStationStatus
{
    Available,
    Occupied,
    OutOfOrder,
    Reserved,
    Unavailable
}
