using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// Heartbeat Request - Sent periodically by charge point
/// </summary>
public class HeartbeatRequest
{
    // Empty payload
}

/// <summary>
/// Heartbeat Response
/// </summary>
public class HeartbeatResponse
{
    [JsonProperty("currentTime")]
    public DateTime CurrentTime { get; set; }
}

