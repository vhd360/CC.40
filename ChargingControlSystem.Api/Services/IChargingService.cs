using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Services;

public interface IChargingService
{
    Task<IEnumerable<ChargingStation>> GetChargingStationsAsync();
    Task<ChargingStation?> GetChargingStationByIdAsync(Guid stationId);
    Task<ChargingSession> StartChargingSessionAsync(Guid chargingPointId, Guid? userId, Guid? vehicleId);
    Task<ChargingSession> StopChargingSessionAsync(Guid sessionId);
    Task<IEnumerable<ChargingSession>> GetChargingSessionsAsync(Guid? userId = null);
    Task<IEnumerable<object>> GetActiveSessionsForUserAsync(Guid userId);
    Task<IEnumerable<object>> GetStationConnectorsAsync(Guid stationId);
    Task ResetConnectorStatusAsync(Guid chargingPointId);
    Task<object> CleanupDuplicateSessionsAsync();
}
