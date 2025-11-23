using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class ChargingStation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ChargingParkId { get; set; }

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

    // OCPP BootNotification fields
    [MaxLength(100)]
    public string? SerialNumber { get; set; } // ChargePointSerialNumber oder ChargeBoxSerialNumber

    [MaxLength(100)]
    public string? FirmwareVersion { get; set; }

    [MaxLength(50)]
    public string? Iccid { get; set; } // SIM-Karten ICCID

    [MaxLength(50)]
    public string? Imsi { get; set; } // SIM-Karten IMSI

    [MaxLength(100)]
    public string? MeterType { get; set; }

    [MaxLength(100)]
    public string? MeterSerialNumber { get; set; }

    // Configuration
    [Column(TypeName = "nvarchar(max)")]
    public string? ConfigurationJson { get; set; } // JSON with configuration key-value pairs

    public DateTime? LastConfigurationUpdate { get; set; }

    // Firmware
    [MaxLength(50)]
    public string? FirmwareStatus { get; set; } // Current firmware status

    public DateTime? LastFirmwareStatusUpdate { get; set; }

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
    public virtual ChargingPark ChargingPark { get; set; } = null!;
    public virtual ICollection<ChargingPoint> ChargingPoints { get; set; } = new List<ChargingPoint>();
    public virtual ICollection<ChargingStationDiagnostics> Diagnostics { get; set; } = new List<ChargingStationDiagnostics>();
    public virtual ICollection<ChargingStationFirmwareHistory> FirmwareHistory { get; set; } = new List<ChargingStationFirmwareHistory>();
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
