using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class ChargingSession
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid ChargingPointId { get; set; }

    public Guid? UserId { get; set; } // null für Adhoc-Ladung

    public Guid? VehicleId { get; set; }

    public Guid? QrCodeId { get; set; } // für QR-Code basierte Sessions

    public Guid? AuthorizationMethodId { get; set; } // RFID, Autocharge, etc.

    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty; // OCPP Session ID

    public int? OcppTransactionId { get; set; } // OCPP Transaction ID (integer für OCPP 1.6)

    [Required]
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Zeitpunkt, wann die Energielieferung beendet wurde (OCPP StopTransaction)
    /// </summary>
    public DateTime? ChargingCompletedAt { get; set; }

    /// <summary>
    /// Zeitpunkt, wann die Session komplett beendet wurde (Stecker gezogen / Connector freigegeben)
    /// </summary>
    public DateTime? EndedAt { get; set; }

    public decimal EnergyDelivered { get; set; } // in kWh

    public decimal Cost { get; set; } // in EUR

    [Required]
    public ChargingSessionStatus Status { get; set; } = ChargingSessionStatus.Charging;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ChargingPoint ChargingPoint { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
    public virtual QrCode? QrCode { get; set; }
    public virtual AuthorizationMethod? AuthorizationMethod { get; set; }
}

public enum ChargingSessionStatus
{
    Charging,
    Completed,
    Stopped,
    Faulted,
    Reserved
}
