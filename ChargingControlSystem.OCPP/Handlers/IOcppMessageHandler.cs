using Newtonsoft.Json.Linq;

namespace ChargingControlSystem.OCPP.Handlers;

/// <summary>
/// Interface for handling OCPP messages
/// </summary>
public interface IOcppMessageHandler
{
    Task<object> HandleMessageAsync(string chargeBoxId, string action, JToken payload);
}


