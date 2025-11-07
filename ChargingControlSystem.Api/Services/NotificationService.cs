using ChargingControlSystem.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChargingControlSystem.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyStationStatusChangedAsync(Guid tenantId, Guid stationId, string status, string? message = null)
    {
        try
        {
            var notification = new
            {
                Type = "station_status_changed",
                StationId = stationId,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients
                .Group($"tenant_{tenantId}")
                .SendAsync("StationStatusChanged", notification);

            _logger.LogDebug("Sent station status notification: Station {StationId}, Status {Status}", stationId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send station status notification");
        }
    }

    public async Task NotifyConnectorStatusChangedAsync(Guid tenantId, Guid connectorId, string status, string? message = null)
    {
        try
        {
            var notification = new
            {
                Type = "connector_status_changed",
                ConnectorId = connectorId,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients
                .Group($"tenant_{tenantId}")
                .SendAsync("ConnectorStatusChanged", notification);

            _logger.LogDebug("Sent connector status notification: Connector {ConnectorId}, Status {Status}", connectorId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send connector status notification");
        }
    }

    public async Task NotifySessionUpdateAsync(Guid userId, Guid sessionId, string status, string? message = null)
    {
        try
        {
            var notification = new
            {
                Type = "session_update",
                SessionId = sessionId,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("SessionUpdate", notification);

            _logger.LogDebug("Sent session update notification: Session {SessionId}, Status {Status}", sessionId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send session update notification");
        }
    }

    public async Task NotifyNewTransactionAsync(Guid tenantId, Guid transactionId)
    {
        try
        {
            var notification = new
            {
                Type = "new_transaction",
                TransactionId = transactionId,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients
                .Group($"tenant_{tenantId}")
                .SendAsync("NewTransaction", notification);

            _logger.LogDebug("Sent new transaction notification: Transaction {TransactionId}", transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new transaction notification");
        }
    }
}

