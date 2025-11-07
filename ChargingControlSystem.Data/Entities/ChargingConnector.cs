using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

/// <summary>
/// Represents a physical connector (cable/plug) on a charging point
/// One ChargingPoint can have multiple Connectors (e.g., CCS and Type2)
/// </summary>
public class ChargingConnector
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ChargingPointId { get; set; }

    /// <summary>
    /// OCPP Connector ID (1-based index within the ChargingPoint)
    /// </summary>
    [Required]
    public int ConnectorId { get; set; }

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
    /// Maximum power in kW
    /// </summary>
    [Required]
    public int MaxPower { get; set; }

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
    /// Current status of the connector
    /// </summary>
    [Required]
    public ConnectorStatus Status { get; set; } = ConnectorStatus.Available;

    /// <summary>
    /// Physical reference/label on the charging point
    /// </summary>
    [MaxLength(50)]
    public string? PhysicalReference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastStatusChange { get; set; }

    // Navigation properties
    public virtual ChargingPoint ChargingPoint { get; set; } = null!;
    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
}

public enum ConnectorStatus
{
    Available,
    Occupied,
    Faulted,
    Unavailable,
    Reserved
}
