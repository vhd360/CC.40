using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// GetDiagnostics Request - Request diagnostics information from charge point
/// </summary>
public class GetDiagnosticsRequest
{
    [JsonProperty("location")]
    public string Location { get; set; } = string.Empty; // URL where diagnostics should be uploaded
    
    [JsonProperty("retries")]
    public int? Retries { get; set; } // Number of retries
    
    [JsonProperty("retryInterval")]
    public int? RetryInterval { get; set; } // Retry interval in seconds
    
    [JsonProperty("startTime")]
    public DateTime? StartTime { get; set; } // Start time for diagnostics
    
    [JsonProperty("stopTime")]
    public DateTime? StopTime { get; set; } // Stop time for diagnostics
}

/// <summary>
/// GetDiagnostics Response
/// </summary>
public class GetDiagnosticsResponse
{
    [JsonProperty("fileName")]
    public string? FileName { get; set; } // Name of diagnostics file if available immediately
}

