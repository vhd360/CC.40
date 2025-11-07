using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using ChargingControlSystem.OCPP.Server;
using ChargingControlSystem.OCPP.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargingControlSystem.Api.Services;

public class ChargingService : IChargingService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly OcppWebSocketServer _ocppServer;
    private readonly ITariffService _tariffService;
    private readonly IBillingService _billingService;
    private readonly ILogger<ChargingService> _logger;

    public ChargingService(
        ApplicationDbContext context, 
        ITenantService tenantService,
        OcppWebSocketServer ocppServer,
        ITariffService tariffService,
        IBillingService billingService,
        ILogger<ChargingService> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _ocppServer = ocppServer;
        _tariffService = tariffService;
        _billingService = billingService;
        _logger = logger;
    }

    public async Task<IEnumerable<ChargingStation>> GetChargingStationsAsync()
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null) return new List<ChargingStation>();

        return await _context.ChargingStations
            .Include(cs => cs.ChargingPark)
            .Where(cs => cs.ChargingPark.TenantId == tenant.Id && cs.IsActive)
            .ToListAsync();
    }

    public async Task<ChargingStation?> GetChargingStationByIdAsync(Guid stationId)
    {
        return await _context.ChargingStations
            .Include(cs => cs.ChargingPark)
            .Include(cs => cs.ChargingPoints)
                .ThenInclude(cp => cp.Connectors)
            .FirstOrDefaultAsync(cs => cs.Id == stationId && cs.IsActive);
    }

    public async Task<ChargingSession> StartChargingSessionAsync(Guid connectorId, Guid? userId, Guid? vehicleId)
    {
        var connector = await _context.ChargingConnectors
            .Include(c => c.ChargingPoint)
                .ThenInclude(cp => cp.ChargingStation)
                    .ThenInclude(cs => cs.ChargingPark)
            .FirstOrDefaultAsync(c => c.Id == connectorId);

        if (connector == null || connector.Status != ConnectorStatus.Available)
        {
            throw new InvalidOperationException("Connector nicht verfügbar");
        }

        // Check for existing active session on this connector
        var existingActiveSession = await _context.ChargingSessions
            .FirstOrDefaultAsync(s => s.ChargingConnectorId == connectorId && 
                                     s.Status == ChargingSessionStatus.Charging);

        if (existingActiveSession != null)
        {
            _logger.LogWarning("Connector {ConnectorId} already has an active session {SessionId}", 
                connectorId, existingActiveSession.Id);
            throw new InvalidOperationException("An diesem Connector läuft bereits ein Ladevorgang");
        }

        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant nicht gefunden");
        }

        // Find or create IdTag for user
        string idTag;
        Guid? authMethodId = null;

        if (userId.HasValue)
        {
            var authMethod = await _context.AuthorizationMethods
                .FirstOrDefaultAsync(am => am.UserId == userId.Value && 
                                          am.Type == AuthorizationMethodType.RFID && 
                                          am.IsActive);

            if (authMethod != null)
            {
                idTag = authMethod.Identifier;
                authMethodId = authMethod.Id;
            }
            else
            {
                // Create virtual RFID tag for web-based charging
                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    throw new InvalidOperationException("Benutzer nicht gefunden");
                }

                idTag = $"WEB_{user.Email}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                var newAuthMethod = new AuthorizationMethod
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    Type = AuthorizationMethodType.RFID,
                    Identifier = idTag,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AuthorizationMethods.Add(newAuthMethod);
                await _context.SaveChangesAsync();
                authMethodId = newAuthMethod.Id;

                _logger.LogInformation("Created virtual IdTag {IdTag} for user {UserId}", idTag, userId);
            }
        }
        else
        {
            // Anonymous/Ad-hoc charging
            idTag = $"ADHOC_{Guid.NewGuid().ToString().Substring(0, 12)}";
        }

        // Send OCPP RemoteStartTransaction to charging station
        var remoteStartRequest = new RemoteStartTransactionRequest
        {
            ConnectorId = connector.ChargingPoint.EvseId,
            IdTag = idTag
        };

        var chargeBoxId = connector.ChargingPoint.ChargingStation?.ChargeBoxId;
        if (string.IsNullOrEmpty(chargeBoxId))
        {
            throw new InvalidOperationException("Ladestation hat keine ChargeBoxId");
        }

        try
        {
            await _ocppServer.SendMessageAsync(
                chargeBoxId,
                "RemoteStartTransaction",
                remoteStartRequest);

            _logger.LogInformation("Sent RemoteStartTransaction to {ChargeBoxId}, EVSE {EvseId}, IdTag {IdTag}",
                chargeBoxId,
                connector.ChargingPoint.EvseId,
                idTag);

            // Create session in database (will be updated by StartTransaction callback)
            var session = new ChargingSession
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ChargingConnectorId = connectorId,
                UserId = userId,
                VehicleId = vehicleId,
                SessionId = Guid.NewGuid().ToString(),
                AuthorizationMethodId = authMethodId,
                StartedAt = DateTime.UtcNow,
                Status = ChargingSessionStatus.Charging
            };

            _context.ChargingSessions.Add(session);
            connector.Status = ConnectorStatus.Occupied;
            await _context.SaveChangesAsync();

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start charging session via OCPP");
            throw new InvalidOperationException($"Fehler beim Starten des Ladevorgangs: {ex.Message}");
        }
    }

    public async Task<ChargingSession> StopChargingSessionAsync(Guid sessionId)
    {
        var session = await _context.ChargingSessions
            .Include(s => s.ChargingConnector)
                .ThenInclude(c => c.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            throw new InvalidOperationException("Session nicht gefunden");
        }

        if (session.Status != ChargingSessionStatus.Charging)
        {
            throw new InvalidOperationException("Session ist nicht aktiv");
        }

        // If session has an OCPP transaction ID, send RemoteStopTransaction
        if (session.OcppTransactionId.HasValue)
        {
            var chargeBoxId = session.ChargingConnector?.ChargingPoint?.ChargingStation?.ChargeBoxId;
            
            if (!string.IsNullOrEmpty(chargeBoxId))
            {
                try
                {
                    var remoteStopRequest = new RemoteStopTransactionRequest
                    {
                        TransactionId = session.OcppTransactionId.Value
                    };

                    await _ocppServer.SendMessageAsync(
                        chargeBoxId,
                        "RemoteStopTransaction",
                        remoteStopRequest);

                _logger.LogInformation("Sent RemoteStopTransaction for session {SessionId}, OCPP TransactionId {TransactionId}",
                    sessionId, session.OcppTransactionId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RemoteStopTransaction, updating session status anyway");
                }
            }
            else
            {
                _logger.LogWarning("Session {SessionId} has no ChargeBoxId, cannot send RemoteStopTransaction", sessionId);
            }
        }
        else
        {
            _logger.LogWarning("Session {SessionId} has no OCPP TransactionId, cannot send RemoteStopTransaction", sessionId);
        }

        // Set ChargingCompletedAt when stopping via Web-UI
        // This marks the end of energy delivery (though the actual end might come later via OCPP)
        session.ChargingCompletedAt = DateTime.UtcNow;

        // Calculate cost and create billing transaction if we have energy data
        // Note: This will be recalculated when StopTransaction arrives from the charging station
        if (session.EnergyDelivered > 0 && session.UserId.HasValue)
        {
            try
            {
                var costCalculation = await _tariffService.CalculateCostAsync(session);
                session.Cost = costCalculation?.TotalCost ?? 0m;
                
                var chargingMinutes = session.ChargingCompletedAt.HasValue 
                    ? (session.ChargingCompletedAt.Value - session.StartedAt).TotalMinutes 
                    : 0;
                var idleMinutes = session.ChargingCompletedAt.HasValue && session.EndedAt.HasValue
                    ? (session.EndedAt.Value - session.ChargingCompletedAt.Value).TotalMinutes 
                    : 0;
                
                _logger.LogInformation("Calculated cost for session {SessionId}: {Cost} EUR (Charging: {ChargingTime}min, Idle: {IdleTime}min)", 
                    sessionId, session.Cost, chargingMinutes, idleMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate cost for session {SessionId}", sessionId);
                // Use fallback calculation
                session.Cost = session.EnergyDelivered * 0.30m;
            }
        }
        else
        {
            if (session.EnergyDelivered <= 0)
            {
                _logger.LogWarning("Session {SessionId} has no energy data yet, cost will be calculated when StopTransaction arrives", 
                    sessionId);
            }
            else
            {
                // No user - use fallback rate
                session.Cost = session.EnergyDelivered * 0.30m;
                _logger.LogInformation("Session {SessionId} has no user, using fallback rate: {Cost} EUR", 
                    sessionId, session.Cost);
            }
        }

        // Update session status
        session.EndedAt = DateTime.UtcNow;
        session.Status = ChargingSessionStatus.Completed;
        
        if (session.ChargingConnector != null)
        {
            session.ChargingConnector.Status = ConnectorStatus.Available;
        }

        await _context.SaveChangesAsync();

        // Create billing transaction if we have cost data
        if (session.Cost > 0)
        {
            try
            {
                await _billingService.CreateTransactionForSessionAsync(session);
                _logger.LogInformation("Created billing transaction for session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create billing transaction for session {SessionId}", sessionId);
                // Don't throw - billing should not block the stop operation
            }
        }

        return session;
    }

    public async Task<IEnumerable<ChargingSession>> GetChargingSessionsAsync(Guid? userId = null)
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null) return new List<ChargingSession>();

        var query = _context.ChargingSessions
            .Include(s => s.ChargingConnector)
                .ThenInclude(c => c.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
            .Where(s => s.TenantId == tenant.Id);

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<object>> GetActiveSessionsForUserAsync(Guid userId)
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null) return new List<object>();

        var activeSessions = await _context.ChargingSessions
            .Include(s => s.ChargingConnector)
                .ThenInclude(c => c.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
                        .ThenInclude(cs => cs.ChargingPark)
            .Include(s => s.Vehicle)
            .Where(s => s.TenantId == tenant.Id && 
                       s.UserId == userId && 
                       s.Status == ChargingSessionStatus.Charging)
            .Select(s => new
            {
                s.Id,
                s.SessionId,
                s.StartedAt,
                DurationMinutes = (int)(DateTime.UtcNow - s.StartedAt).TotalMinutes,
                Station = new
                {
                    s.ChargingConnector.ChargingPoint.ChargingStation.Id,
                    s.ChargingConnector.ChargingPoint.ChargingStation.Name,
                    s.ChargingConnector.ChargingPoint.ChargingStation.StationId,
                    ChargingPark = new
                    {
                        s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Name,
                        s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.Address,
                        s.ChargingConnector.ChargingPoint.ChargingStation.ChargingPark.City
                    }
                },
                Connector = new
                {
                    s.ChargingConnector.Id,
                    s.ChargingConnector.ConnectorId,
                    Type = s.ChargingConnector.ConnectorType.ToString()
                },
                Vehicle = s.Vehicle != null ? new
                {
                    s.Vehicle.Id,
                    s.Vehicle.Make,
                    s.Vehicle.Model,
                    s.Vehicle.LicensePlate
                } : null,
                s.EnergyDelivered,
                s.Cost
            })
            .ToListAsync();

        return activeSessions;
    }

    public async Task<IEnumerable<object>> GetStationConnectorsAsync(Guid stationId)
    {
        var connectors = await _context.ChargingConnectors
            .Include(c => c.ChargingPoint)
            .Where(c => c.ChargingPoint.ChargingStationId == stationId)
            .OrderBy(c => c.ChargingPoint.EvseId)
            .ThenBy(c => c.ConnectorId)
            .Select(c => new
            {
                c.Id,
                c.ConnectorId,
                EvseId = c.ChargingPoint.EvseId,
                PointName = c.ChargingPoint.Name,
                Type = c.ConnectorType.ToString(),
                Status = c.Status.ToString(),
                MaxPower = c.ChargingPoint.MaxPower,
                IsAvailable = c.Status == ConnectorStatus.Available
            })
            .ToListAsync();

        return connectors;
    }

    public async Task ResetConnectorStatusAsync(Guid connectorId)
    {
        var connector = await _context.ChargingConnectors
            .FirstOrDefaultAsync(c => c.Id == connectorId);

        if (connector == null)
        {
            throw new InvalidOperationException("Connector not found");
        }

        // Check if there's an active session
        var activeSession = await _context.ChargingSessions
            .FirstOrDefaultAsync(s => s.ChargingConnectorId == connectorId && 
                                     s.Status == ChargingSessionStatus.Charging);

        if (activeSession != null)
        {
            throw new InvalidOperationException("Cannot reset connector with active session. Stop the session first.");
        }

        connector.Status = ConnectorStatus.Available;
        await _context.SaveChangesAsync();
    }

    public async Task<object> CleanupDuplicateSessionsAsync()
    {
        var tenant = await _tenantService.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        // Find all active sessions grouped by connector
        var activeSessions = await _context.ChargingSessions
            .Where(s => s.TenantId == tenant.Id && s.Status == ChargingSessionStatus.Charging)
            .OrderBy(s => s.StartedAt) // Oldest first
            .ToListAsync();

        var duplicatesFound = new List<object>();
        var duplicatesByConnector = activeSessions
            .GroupBy(s => s.ChargingConnectorId)
            .Where(g => g.Count() > 1);

        foreach (var group in duplicatesByConnector)
        {
            var sessions = group.ToList();
            var keepSession = sessions.First(); // Keep the oldest
            var removeSessions = sessions.Skip(1).ToList();

            foreach (var duplicateSession in removeSessions)
            {
                duplicateSession.Status = ChargingSessionStatus.Stopped;
                duplicateSession.EndedAt = DateTime.UtcNow;
                
                duplicatesFound.Add(new
                {
                    SessionId = duplicateSession.Id,
                    ConnectorId = duplicateSession.ChargingConnectorId,
                    StartedAt = duplicateSession.StartedAt,
                    Action = "Stopped (Duplicate)"
                });
            }

            _logger.LogWarning("Found {Count} duplicate sessions on connector {ConnectorId}. Kept session {KeepId}, cancelled {CancelCount}",
                sessions.Count, group.Key, keepSession.Id, removeSessions.Count);
        }

        await _context.SaveChangesAsync();

        return new
        {
            message = $"Bereinigung abgeschlossen. {duplicatesFound.Count} doppelte Session(s) wurden storniert.",
            duplicatesRemoved = duplicatesFound
        };
    }
}
