using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// StartTransaction Request - Sent when charging starts
/// </summary>
public class StartTransactionRequest
{
    [JsonProperty("connectorId")]
    public int ConnectorId { get; set; }
    
    [JsonProperty("idTag")]
    public string IdTag { get; set; } = string.Empty;
    
    [JsonProperty("meterStart")]
    public int MeterStart { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonProperty("reservationId")]
    public int? ReservationId { get; set; }
}

/// <summary>
/// StartTransaction Response
/// </summary>
public class StartTransactionResponse
{
    [JsonProperty("idTagInfo")]
    public IdTagInfo IdTagInfo { get; set; } = new();
    
    [JsonProperty("transactionId")]
    public int TransactionId { get; set; }
}

/// <summary>
/// StopTransaction Request - Sent when charging stops
/// </summary>
public class StopTransactionRequest
{
    [JsonProperty("transactionId")]
    public int TransactionId { get; set; }
    
    [JsonProperty("meterStop")]
    public int MeterStop { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonProperty("reason")]
    public string? Reason { get; set; }
    
    [JsonProperty("idTag")]
    public string? IdTag { get; set; }
    
    [JsonProperty("transactionData")]
    public List<MeterValue>? TransactionData { get; set; }
}

/// <summary>
/// StopTransaction Response
/// </summary>
public class StopTransactionResponse
{
    [JsonProperty("idTagInfo")]
    public IdTagInfo? IdTagInfo { get; set; }
}

/// <summary>
/// IdTag authorization info
/// </summary>
public class IdTagInfo
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonProperty("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
    
    [JsonProperty("parentIdTag")]
    public string? ParentIdTag { get; set; }
}

/// <summary>
/// Meter values
/// </summary>
public class MeterValue
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonProperty("sampledValue")]
    public List<SampledValue> SampledValue { get; set; } = new();
}

public class SampledValue
{
    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonProperty("context")]
    public string? Context { get; set; }
    
    [JsonProperty("format")]
    public string? Format { get; set; }
    
    [JsonProperty("measurand")]
    public string? Measurand { get; set; }
    
    [JsonProperty("phase")]
    public string? Phase { get; set; }
    
    [JsonProperty("location")]
    public string? Location { get; set; }
    
    [JsonProperty("unit")]
    public string? Unit { get; set; }
}

public static class AuthorizationStatus
{
    public const string Accepted = "Accepted";
    public const string Blocked = "Blocked";
    public const string Expired = "Expired";
    public const string Invalid = "Invalid";
    public const string ConcurrentTx = "ConcurrentTx";
}

public static class StopReason
{
    public const string EmergencyStop = "EmergencyStop";
    public const string EVDisconnected = "EVDisconnected";
    public const string HardReset = "HardReset";
    public const string Local = "Local";
    public const string Other = "Other";
    public const string PowerLoss = "PowerLoss";
    public const string Reboot = "Reboot";
    public const string Remote = "Remote";
    public const string SoftReset = "SoftReset";
    public const string UnlockCommand = "UnlockCommand";
    public const string DeAuthorized = "DeAuthorized";
}
