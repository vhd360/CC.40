using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

/// <summary>
/// Stores diagnostics information requests and results for charging stations
/// </summary>
public class ChargingStationDiagnostics
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ChargingStationId { get; set; }

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    [MaxLength(500)]
    public string? DiagnosticsUrl { get; set; } // URL where diagnostics file is stored

    [MaxLength(100)]
    public string? FileName { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending | Completed | Failed

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    public DateTime? StartTime { get; set; } // Diagnostics start time

    public DateTime? StopTime { get; set; } // Diagnostics stop time

    // Navigation property
    [ForeignKey("ChargingStationId")]
    public virtual ChargingStation ChargingStation { get; set; } = null!;
}

public static class DiagnosticsStatus
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

