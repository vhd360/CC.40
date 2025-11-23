using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

/// <summary>
/// Stores firmware update history for charging stations
/// </summary>
public class ChargingStationFirmwareHistory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ChargingStationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirmwareVersion { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // Downloaded | DownloadFailed | Downloading | Idle | InstallationFailed | Installing | Installed

    [MaxLength(1000)]
    public string? Info { get; set; } // Additional information about the status

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("ChargingStationId")]
    public virtual ChargingStation ChargingStation { get; set; } = null!;
}

