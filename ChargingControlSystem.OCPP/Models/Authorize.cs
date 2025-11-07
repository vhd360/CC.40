using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// Authorize Request - Sent to authorize an ID tag
/// </summary>
public class AuthorizeRequest
{
    [JsonProperty("idTag")]
    public string IdTag { get; set; } = string.Empty;
}

/// <summary>
/// Authorize Response
/// </summary>
public class AuthorizeResponse
{
    [JsonProperty("idTagInfo")]
    public IdTagInfo IdTagInfo { get; set; } = new();
}
