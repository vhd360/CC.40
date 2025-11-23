using ChargingControlSystem.OCPP.Models;
using ChargingControlSystem.OCPP.Server;
using Microsoft.Extensions.Logging;

namespace ChargingControlSystem.Api.Services;

public class OcppCommandService : IOcppCommandService
{
    private readonly OcppWebSocketServer _ocppServer;
    private readonly ILogger<OcppCommandService> _logger;

    public OcppCommandService(OcppWebSocketServer ocppServer, ILogger<OcppCommandService> logger)
    {
        _ocppServer = ocppServer;
        _logger = logger;
    }

    public async Task<GetConfigurationResponse> GetConfigurationAsync(string chargeBoxId, List<string>? keys = null)
    {
        try
        {
            var request = new GetConfigurationRequest
            {
                Key = keys
            };

            // Send command and wait for response
            // Note: In a real implementation, you'd need to handle async response waiting
            // For now, we'll use a simple approach - the response will come back through HandleGetConfigurationAsync
            await _ocppServer.SendMessageAsync(chargeBoxId, "GetConfiguration", request);
            
            _logger.LogInformation("GetConfiguration command sent to {ChargeBoxId}", chargeBoxId);
            
            // Return a response indicating the command was sent
            // The actual configuration will be retrieved from the database after the station responds
            return new GetConfigurationResponse
            {
                ConfigurationKey = new List<ConfigurationKey>(),
                UnknownKey = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send GetConfiguration to {ChargeBoxId}", chargeBoxId);
            throw;
        }
    }

    public async Task<ChangeConfigurationResponse> ChangeConfigurationAsync(string chargeBoxId, string key, string value)
    {
        try
        {
            var request = new ChangeConfigurationRequest
            {
                Key = key,
                Value = value
            };

            await _ocppServer.SendMessageAsync(chargeBoxId, "ChangeConfiguration", request);
            
            _logger.LogInformation("ChangeConfiguration command sent to {ChargeBoxId}: {Key} = {Value}", chargeBoxId, key, value);
            
            // Return accepted - actual status will be updated when station responds
            return new ChangeConfigurationResponse
            {
                Status = ConfigurationStatus.Accepted
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send ChangeConfiguration to {ChargeBoxId}", chargeBoxId);
            throw;
        }
    }

    public async Task<GetDiagnosticsResponse> RequestDiagnosticsAsync(string chargeBoxId, string location, DateTime? startTime = null, DateTime? stopTime = null)
    {
        try
        {
            var request = new GetDiagnosticsRequest
            {
                Location = location,
                StartTime = startTime,
                StopTime = stopTime,
                Retries = 3,
                RetryInterval = 60
            };

            await _ocppServer.SendMessageAsync(chargeBoxId, "GetDiagnostics", request);
            
            _logger.LogInformation("GetDiagnostics command sent to {ChargeBoxId}", chargeBoxId);
            
            return new GetDiagnosticsResponse
            {
                FileName = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send GetDiagnostics to {ChargeBoxId}", chargeBoxId);
            throw;
        }
    }
}

