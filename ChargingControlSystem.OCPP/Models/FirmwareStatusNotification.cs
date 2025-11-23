using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// FirmwareStatusNotification Request - Sent to report the status of a firmware update
/// </summary>
public class FirmwareStatusNotificationRequest
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonProperty("info")]
    public string? Info { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// FirmwareStatusNotification Response
/// </summary>
public class FirmwareStatusNotificationResponse
{
    // Empty payload
}

public static class FirmwareStatus
{
    public const string Downloaded = "Downloaded";
    public const string DownloadFailed = "DownloadFailed";
    public const string Downloading = "Downloading";
    public const string Idle = "Idle";
    public const string InstallationFailed = "InstallationFailed";
    public const string Installing = "Installing";
    public const string Installed = "Installed";
}



