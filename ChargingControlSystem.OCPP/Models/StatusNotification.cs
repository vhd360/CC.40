using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// StatusNotification Request - Sent when connector status changes
/// </summary>
public class StatusNotificationRequest
{
    [JsonProperty("connectorId")]
    public int ConnectorId { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonProperty("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;
    
    [JsonProperty("info")]
    public string? Info { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime? Timestamp { get; set; }
    
    [JsonProperty("vendorId")]
    public string? VendorId { get; set; }
    
    [JsonProperty("vendorErrorCode")]
    public string? VendorErrorCode { get; set; }
}

/// <summary>
/// StatusNotification Response
/// </summary>
public class StatusNotificationResponse
{
    // Empty payload
}

public static class ChargePointStatus
{
    public const string Available = "Available";
    public const string Preparing = "Preparing";
    public const string Charging = "Charging";
    public const string SuspendedEVSE = "SuspendedEVSE";
    public const string SuspendedEV = "SuspendedEV";
    public const string Finishing = "Finishing";
    public const string Reserved = "Reserved";
    public const string Unavailable = "Unavailable";
    public const string Faulted = "Faulted";
}

public static class ChargePointErrorCode
{
    public const string NoError = "NoError";
    public const string ConnectorLockFailure = "ConnectorLockFailure";
    public const string EVCommunicationError = "EVCommunicationError";
    public const string GroundFailure = "GroundFailure";
    public const string HighTemperature = "HighTemperature";
    public const string InternalError = "InternalError";
    public const string LocalListConflict = "LocalListConflict";
    public const string OverCurrentFailure = "OverCurrentFailure";
    public const string OverVoltage = "OverVoltage";
    public const string PowerMeterFailure = "PowerMeterFailure";
    public const string PowerSwitchFailure = "PowerSwitchFailure";
    public const string ReaderFailure = "ReaderFailure";
    public const string ResetFailure = "ResetFailure";
    public const string UnderVoltage = "UnderVoltage";
    public const string WeakSignal = "WeakSignal";
    public const string OtherError = "OtherError";
}
