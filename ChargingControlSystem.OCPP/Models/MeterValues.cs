using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// MeterValues Request - Sent periodically with meter readings
/// </summary>
public class MeterValuesRequest
{
    [JsonProperty("connectorId")]
    public int ConnectorId { get; set; }
    
    [JsonProperty("transactionId")]
    public int? TransactionId { get; set; }
    
    [JsonProperty("meterValue")]
    public List<MeterValue> MeterValue { get; set; } = new();
}

/// <summary>
/// MeterValues Response
/// </summary>
public class MeterValuesResponse
{
    // Empty payload
}
