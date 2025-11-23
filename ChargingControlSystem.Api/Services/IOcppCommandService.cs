using ChargingControlSystem.OCPP.Models;

namespace ChargingControlSystem.Api.Services;

public interface IOcppCommandService
{
    Task<GetConfigurationResponse> GetConfigurationAsync(string chargeBoxId, List<string>? keys = null);
    Task<ChangeConfigurationResponse> ChangeConfigurationAsync(string chargeBoxId, string key, string value);
    Task<GetDiagnosticsResponse> RequestDiagnosticsAsync(string chargeBoxId, string location, DateTime? startTime = null, DateTime? stopTime = null);
}

