using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// BootNotification Request - Sent by charge point after boot
/// </summary>
public class BootNotificationRequest
{
    [JsonProperty("chargePointVendor")]
    public string ChargePointVendor { get; set; } = string.Empty;
    
    [JsonProperty("chargePointModel")]
    public string ChargePointModel { get; set; } = string.Empty;
    
    [JsonProperty("chargePointSerialNumber")]
    public string? ChargePointSerialNumber { get; set; }
    
    [JsonProperty("chargeBoxSerialNumber")]
    public string? ChargeBoxSerialNumber { get; set; }
    
    [JsonProperty("firmwareVersion")]
    public string? FirmwareVersion { get; set; }
    
    [JsonProperty("iccid")]
    public string? Iccid { get; set; }
    
    [JsonProperty("imsi")]
    public string? Imsi { get; set; }
    
    [JsonProperty("meterType")]
    public string? MeterType { get; set; }
    
    [JsonProperty("meterSerialNumber")]
    public string? MeterSerialNumber { get; set; }
}

/// <summary>
/// BootNotification Response
/// </summary>
public class BootNotificationResponse
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty; // Accepted | Pending | Rejected
    
    [JsonProperty("currentTime")]
    public DateTime CurrentTime { get; set; }
    
    [JsonProperty("interval")]
    public int Interval { get; set; } // Heartbeat interval in seconds
}

public static class RegistrationStatus
{
    public const string Accepted = "Accepted";
    public const string Pending = "Pending";
    public const string Rejected = "Rejected";
}

