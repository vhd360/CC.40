using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

/// <summary>
/// Represents a charging point (EVSE - Electric Vehicle Supply Equipment)
/// One ChargingStation can have multiple ChargingPoints
/// One ChargingPoint = One Connector (1:1 Beziehung)
/// </summary>
public class ChargingPoint
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ChargingStationId { get; set; }

    /// <summary>
    /// OCPP ConnectorId (1-based index) - used for OCPP communication
    /// </summary>
    [Required]
    public int EvseId { get; set; }

    /// <summary>
    /// EVSE-ID according to ISO 15118 and OCPI
    /// Format: DE*ABC*E1234*5678 (Country*OperatorId*E[PowerOutletId]*[StationId])
    /// </summary>
    [MaxLength(48)]
    public string? EvseIdExternal { get; set; }

    /// <summary>
    /// Human-readable name for this charging point
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Maximum power output of this charging point in kW
    /// </summary>
    [Required]
    public int MaxPower { get; set; }

    /// <summary>
    /// Connector type: Type2, CCS, CHAdeMO, Tesla, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ConnectorType { get; set; } = string.Empty;

    /// <summary>
    /// Connector format according to OCPI
    /// </summary>
    [MaxLength(50)]
    public string? ConnectorFormat { get; set; } // SOCKET, CABLE, etc.

    /// <summary>
    /// Power type: AC_1_PHASE, AC_3_PHASE, DC
    /// </summary>
    [MaxLength(20)]
    public string? PowerType { get; set; }

    /// <summary>
    /// Maximum current in Amperes
    /// </summary>
    [Required]
    public int MaxCurrent { get; set; }

    /// <summary>
    /// Maximum voltage in Volts
    /// </summary>
    [Required]
    public int MaxVoltage { get; set; }

    /// <summary>
    /// Physical reference/label on the charging point
    /// </summary>
    [MaxLength(50)]
    public string? PhysicalReference { get; set; }

    /// <summary>
    /// Current status of the charging point/connector
    /// </summary>
    [Required]
    public ChargingPointStatus Status { get; set; } = ChargingPointStatus.Offline;

    /// <summary>
    /// Public Key for ISO 15118 Plug & Charge
    /// PEM-encoded X.509 certificate
    /// </summary>
    [MaxLength(4000)]
    public string? PublicKey { get; set; }

    /// <summary>
    /// Certificate chain for Plug & Charge authentication
    /// </summary>
    [MaxLength(8000)]
    public string? CertificateChain { get; set; }

    /// <summary>
    /// Smart charging capabilities
    /// </summary>
    public bool SupportsSmartCharging { get; set; } = false;

    /// <summary>
    /// Supports remote start/stop via OCPP
    /// </summary>
    public bool SupportsRemoteStartStop { get; set; } = true;

    /// <summary>
    /// Supports reservations
    /// </summary>
    public bool SupportsReservation { get; set; } = false;

    /// <summary>
    /// Tariff information as JSON
    /// </summary>
    [MaxLength(2000)]
    public string? TariffInfo { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastStatusChange { get; set; }

    // Navigation properties
    public virtual ChargingStation ChargingStation { get; set; } = null!;
    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
}

public enum ChargingPointStatus
{
    /// <summary>
    /// Available and ready for charging
    /// </summary>
    Available,

    /// <summary>
    /// Occupied - vehicle is connected
    /// </summary>
    Occupied,

    /// <summary>
    /// Charging in progress
    /// </summary>
    Charging,

    /// <summary>
    /// Reserved for a specific user
    /// </summary>
    Reserved,

    /// <summary>
    /// Out of order / faulted
    /// </summary>
    Faulted,

    /// <summary>
    /// Unavailable (e.g., maintenance)
    /// </summary>
    Unavailable,

    /// <summary>
    /// Preparing for charging
    /// </summary>
    Preparing,

    /// <summary>
    /// Charging has finished, waiting for cable disconnect
    /// </summary>
    Finishing,

    /// <summary>
    /// Station/Point has never connected or is offline
    /// </summary>
    Offline
}

