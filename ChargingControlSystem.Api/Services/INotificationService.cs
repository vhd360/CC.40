namespace ChargingControlSystem.Api.Services;

public interface INotificationService
{
    /// <summary>
    /// Notify all clients in a tenant about a station status change
    /// </summary>
    Task NotifyStationStatusChangedAsync(Guid tenantId, Guid stationId, string status, string? message = null);

    /// <summary>
    /// Notify all clients in a tenant about a connector status change
    /// </summary>
    Task NotifyConnectorStatusChangedAsync(Guid tenantId, Guid connectorId, string status, string? message = null);

    /// <summary>
    /// Notify a specific user about a session update
    /// </summary>
    Task NotifySessionUpdateAsync(Guid userId, Guid sessionId, string status, string? message = null);

    /// <summary>
    /// Notify all clients in a tenant about a new billing transaction
    /// </summary>
    Task NotifyNewTransactionAsync(Guid tenantId, Guid transactionId);
}

