using Newtonsoft.Json;

namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// GetConfiguration Request - Request configuration keys from charge point
/// </summary>
public class GetConfigurationRequest
{
    [JsonProperty("key")]
    public List<string>? Key { get; set; } // Optional: specific keys to retrieve, null = all keys
}

/// <summary>
/// Configuration Key-Value Pair
/// </summary>
public class ConfigurationKey
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonProperty("value")]
    public string? Value { get; set; }
    
    [JsonProperty("readonly")]
    public bool Readonly { get; set; }
}

/// <summary>
/// GetConfiguration Response
/// </summary>
public class GetConfigurationResponse
{
    [JsonProperty("configurationKey")]
    public List<ConfigurationKey> ConfigurationKey { get; set; } = new();
    
    [JsonProperty("unknownKey")]
    public List<string>? UnknownKey { get; set; }
}

/// <summary>
/// ChangeConfiguration Request - Change configuration value
/// </summary>
public class ChangeConfigurationRequest
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// ChangeConfiguration Response
/// </summary>
public class ChangeConfigurationResponse
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty; // Accepted | Rejected | RebootRequired | NotSupported
}

public static class ConfigurationStatus
{
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string RebootRequired = "RebootRequired";
    public const string NotSupported = "NotSupported";
}

