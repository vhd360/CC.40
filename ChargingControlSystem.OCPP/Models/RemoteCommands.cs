using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// RemoteStartTransaction Request - Sent by Central System to start charging
/// </summary>
public class RemoteStartTransactionRequest
{
    [JsonProperty("connectorId")]
    public int? ConnectorId { get; set; }
    
    [JsonProperty("idTag")]
    public string IdTag { get; set; } = string.Empty;
    
    [JsonProperty("chargingProfile")]
    public ChargingProfile? ChargingProfile { get; set; }
}

/// <summary>
/// RemoteStartTransaction Response
/// </summary>
public class RemoteStartTransactionResponse
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty; // "Accepted" or "Rejected"
}

/// <summary>
/// RemoteStopTransaction Request - Sent by Central System to stop charging
/// </summary>
public class RemoteStopTransactionRequest
{
    [JsonProperty("transactionId")]
    public int TransactionId { get; set; }
}

/// <summary>
/// RemoteStopTransaction Response
/// </summary>
public class RemoteStopTransactionResponse
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty; // "Accepted" or "Rejected"
}

/// <summary>
/// Charging Profile (optional, for smart charging)
/// </summary>
public class ChargingProfile
{
    [JsonProperty("chargingProfileId")]
    public int ChargingProfileId { get; set; }
    
    [JsonProperty("stackLevel")]
    public int StackLevel { get; set; }
    
    [JsonProperty("chargingProfilePurpose")]
    public string ChargingProfilePurpose { get; set; } = string.Empty;
    
    [JsonProperty("chargingProfileKind")]
    public string ChargingProfileKind { get; set; } = string.Empty;
}

