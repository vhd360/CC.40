using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChargingControlSystem.OCPP.Server;

namespace ChargingControlSystem.OCPP.Services;

/// <summary>
/// Background service that runs the OCPP WebSocket server
/// </summary>
public class OcppHostedService : IHostedService
{
    private readonly OcppWebSocketServer _ocppServer;
    private readonly ILogger<OcppHostedService> _logger;
    private Task? _executingTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public OcppHostedService(
        OcppWebSocketServer ocppServer,
        ILogger<OcppHostedService> logger)
    {
        _ocppServer = ocppServer;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting OCPP Server...");
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = _ocppServer.StartAsync(_cancellationTokenSource.Token);

        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _cancellationTokenSource?.Cancel();
            await _ocppServer.StopAsync();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        _logger.LogInformation("OCPP Server stopped");
    }
}


