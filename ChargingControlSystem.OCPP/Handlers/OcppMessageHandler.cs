using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using ChargingControlSystem.Data.Enums;
using ChargingControlSystem.OCPP.Models;

namespace ChargingControlSystem.OCPP.Handlers;

/// <summary>
/// Handles OCPP 1.6J messages and persists data to database
/// </summary>
public class OcppMessageHandler : IOcppMessageHandler
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<OcppMessageHandler> _logger;
    private readonly Func<IServiceProvider>? _serviceProviderFactory;

    public OcppMessageHandler(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<OcppMessageHandler> logger,
        Func<IServiceProvider>? serviceProviderFactory = null)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _serviceProviderFactory = serviceProviderFactory;
    }

    public async Task<object> HandleMessageAsync(string chargeBoxId, string action, JToken payload)
    {
        // Configure JSON deserializer to treat all timestamps as UTC
        var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc
        };
        var serializer = Newtonsoft.Json.JsonSerializer.Create(jsonSettings);

        return action switch
        {
            "BootNotification" => await HandleBootNotificationAsync(chargeBoxId, payload.ToObject<BootNotificationRequest>(serializer)!),
            "Heartbeat" => await HandleHeartbeatAsync(chargeBoxId),
            "StatusNotification" => await HandleStatusNotificationAsync(chargeBoxId, payload.ToObject<StatusNotificationRequest>(serializer)!),
            "Authorize" => await HandleAuthorizeAsync(chargeBoxId, payload.ToObject<AuthorizeRequest>(serializer)!),
            "StartTransaction" => await HandleStartTransactionAsync(chargeBoxId, payload.ToObject<StartTransactionRequest>(serializer)!),
            "StopTransaction" => await HandleStopTransactionAsync(chargeBoxId, payload.ToObject<StopTransactionRequest>(serializer)!),
            "MeterValues" => await HandleMeterValuesAsync(chargeBoxId, payload.ToObject<MeterValuesRequest>(serializer)!),
            _ => throw new NotImplementedException($"Action {action} not implemented")
        };
    }

    private async Task<BootNotificationResponse> HandleBootNotificationAsync(string chargeBoxId, BootNotificationRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Find charging station by ChargeBoxId
        var station = await context.ChargingStations
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station == null)
        {
            _logger.LogWarning("Unknown ChargeBox: {ChargeBoxId}", chargeBoxId);
            return new BootNotificationResponse
            {
                Status = RegistrationStatus.Rejected,
                CurrentTime = DateTime.UtcNow,
                Interval = 300 // Retry in 5 minutes
            };
        }

        // Update station information
        station.Vendor = request.ChargePointVendor;
        station.Model = request.ChargePointModel;
        station.LastHeartbeat = DateTime.UtcNow;
        station.Status = ChargingStationStatus.Available;

        await context.SaveChangesAsync();

        _logger.LogInformation("BootNotification from {ChargeBoxId}: {Vendor} {Model}", 
            chargeBoxId, request.ChargePointVendor, request.ChargePointModel);

        return new BootNotificationResponse
        {
            Status = RegistrationStatus.Accepted,
            CurrentTime = DateTime.UtcNow,
            Interval = 300 // Heartbeat every 5 minutes
        };
    }

    private async Task<HeartbeatResponse> HandleHeartbeatAsync(string chargeBoxId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var station = await context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station != null)
        {
            var previousStatus = station.Status;
            station.LastHeartbeat = DateTime.UtcNow;
            
            // If station was unavailable and now sending heartbeat, mark as available
            if (previousStatus == ChargingStationStatus.Unavailable)
            {
                station.Status = ChargingStationStatus.Available;
                await context.SaveChangesAsync();
                
                // Notify clients about status change
                await NotifyStationStatusChangedAsync(station.ChargingPark.TenantId, station.Id, "Available", "Station is back online");
            }
            else
            {
                await context.SaveChangesAsync();
            }
        }

        _logger.LogDebug("Heartbeat from {ChargeBoxId}", chargeBoxId);

        return new HeartbeatResponse
        {
            CurrentTime = DateTime.UtcNow
        };
    }

    private async Task<StatusNotificationResponse> HandleStatusNotificationAsync(string chargeBoxId, StatusNotificationRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var station = await context.ChargingStations
            .Include(s => s.ChargingPark)
            .Include(s => s.ChargingPoints)
                .ThenInclude(cp => cp.Connectors)
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station != null)
        {
            var previousStationStatus = station.Status;
            
            // Map OCPP status to our enum
            var newStatus = request.Status switch
            {
                ChargePointStatus.Available => ChargingStationStatus.Available,
                ChargePointStatus.Preparing or ChargePointStatus.Charging => ChargingStationStatus.Occupied,
                ChargePointStatus.Unavailable => ChargingStationStatus.Unavailable,
                ChargePointStatus.Faulted => ChargingStationStatus.OutOfOrder,
                ChargePointStatus.Reserved => ChargingStationStatus.Reserved,
                _ => ChargingStationStatus.Unavailable
            };
            
            station.Status = newStatus;

            // Update connector status if ConnectorId > 0
            if (request.ConnectorId > 0)
            {
                var chargingPoint = station.ChargingPoints.FirstOrDefault(cp => cp.EvseId == request.ConnectorId);
                if (chargingPoint != null)
                {
                    var connector = chargingPoint.Connectors.FirstOrDefault();
                    if (connector != null)
                    {
                        var previousConnectorStatus = connector.Status;
                        
                        connector.Status = request.Status switch
                        {
                            ChargePointStatus.Available => ConnectorStatus.Available,
                            ChargePointStatus.Preparing or ChargePointStatus.Charging => ConnectorStatus.Occupied,
                            ChargePointStatus.Unavailable => ConnectorStatus.Unavailable,
                            ChargePointStatus.Faulted => ConnectorStatus.Faulted,
                            ChargePointStatus.Reserved => ConnectorStatus.Reserved,
                            _ => ConnectorStatus.Unavailable
                        };

                        // Notify about connector status change if it changed
                        if (previousConnectorStatus != connector.Status)
                        {
                            await NotifyConnectorStatusChangedAsync(
                                station.ChargingPark.TenantId, 
                                connector.Id, 
                                connector.Status.ToString(), 
                                request.ErrorCode);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();

            // Notify about station status change if it changed
            if (previousStationStatus != newStatus)
            {
                await NotifyStationStatusChangedAsync(
                    station.ChargingPark.TenantId, 
                    station.Id, 
                    newStatus.ToString(), 
                    request.ErrorCode);
            }
        }

        _logger.LogInformation("StatusNotification from {ChargeBoxId}: Connector {ConnectorId} = {Status}", 
            chargeBoxId, request.ConnectorId, request.Status);

        return new StatusNotificationResponse();
    }

    private async Task<AuthorizeResponse> HandleAuthorizeAsync(string chargeBoxId, AuthorizeRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Check if the IdTag (RFID) exists in authorization methods
        var authMethod = await context.AuthorizationMethods
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Identifier == request.IdTag && a.Type == AuthorizationMethodType.RFID && a.IsActive);

        if (authMethod == null || !authMethod.User.IsActive)
        {
            _logger.LogWarning("Authorization failed for IdTag: {IdTag} - Method not found or inactive", request.IdTag);
            return new AuthorizeResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid }
            };
        }

        _logger.LogInformation("Authorization successful for IdTag: {IdTag} (User: {UserId})", 
            request.IdTag, authMethod.UserId);

        return new AuthorizeResponse
        {
            IdTagInfo = new IdTagInfo
            {
                Status = AuthorizationStatus.Accepted,
                ExpiryDate = null
            }
        };
    }

    private async Task<StartTransactionResponse> HandleStartTransactionAsync(string chargeBoxId, StartTransactionRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Find station with charging points and connectors
        var station = await context.ChargingStations
            .Include(s => s.ChargingPoints)
                .ThenInclude(cp => cp.Connectors)
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station == null)
        {
            _logger.LogWarning("Station not found for ChargeBox: {ChargeBoxId}", chargeBoxId);
            return new StartTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                TransactionId = 0
            };
        }

        // Find authorization method
        var authMethod = await context.AuthorizationMethods
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Identifier == request.IdTag && a.Type == AuthorizationMethodType.RFID);

        if (authMethod == null || !authMethod.IsActive)
        {
            _logger.LogWarning("Authorization method not found: {IdTag}", request.IdTag);
            return new StartTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                TransactionId = 0
            };
        }

        // Check if user has access to this station via UserGroup permissions
        var hasAccess = await CheckUserStationAccessAsync(context, authMethod.UserId, station.Id);
        if (!hasAccess)
        {
            _logger.LogWarning("User {UserId} has no access to station {StationId} via user groups", 
                authMethod.UserId, station.Id);
            return new StartTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Blocked },
                TransactionId = 0
            };
        }

        // Find charging point by OCPP connector ID (which maps to EvseId)
        var chargingPoint = station.ChargingPoints.FirstOrDefault(cp => cp.EvseId == request.ConnectorId);
        if (chargingPoint == null)
        {
            _logger.LogWarning("ChargingPoint with EvseId {ConnectorId} not found on station {StationId}", 
                request.ConnectorId, station.Id);
            return new StartTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                TransactionId = 0
            };
        }

        // Find connector (either Available or Occupied - might be occupied from Web-UI start)
        var connector = chargingPoint.Connectors.FirstOrDefault(c => 
            c.Status == ConnectorStatus.Available || c.Status == ConnectorStatus.Occupied);
        
        if (connector == null)
        {
            _logger.LogWarning("No connector found on ChargingPoint {ChargingPointId} (EvseId: {EvseId})", 
                chargingPoint.Id, chargingPoint.EvseId);
            return new StartTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                TransactionId = 0
            };
        }

        // Get tenant ID from charging park
        var park = await context.ChargingParks.FindAsync(station.ChargingParkId);
        if (park == null)
        {
            _logger.LogWarning("Charging park not found for station {StationId}", station.Id);
            return new StartTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                TransactionId = 0
            };
        }

        // Generate unique OCPP transaction ID
        var random = new Random();
        var ocppTransactionId = random.Next(1, int.MaxValue);

        // Check if there's already a session for this connector (from Web-UI RemoteStart)
        var existingSession = await context.ChargingSessions
            .FirstOrDefaultAsync(s => 
                s.ChargingConnectorId == connector.Id && 
                s.Status == ChargingSessionStatus.Charging &&
                s.AuthorizationMethodId == authMethod.Id &&
                s.OcppTransactionId == null);

        ChargingSession session;
        
        if (existingSession != null)
        {
            // Update existing session with OCPP transaction ID
            existingSession.OcppTransactionId = ocppTransactionId;
            existingSession.StartedAt = request.Timestamp;
            session = existingSession;
            
            _logger.LogInformation("Updated existing session {SessionId} with OcppTransactionId={TransactionId}", 
                session.Id, ocppTransactionId);
        }
        else
        {
            // Create new charging session (traditional RFID start)
            session = new ChargingSession
            {
                Id = Guid.NewGuid(),
                TenantId = park.TenantId,
                ChargingConnectorId = connector.Id,
                UserId = authMethod.UserId,
                SessionId = Guid.NewGuid().ToString(),
                OcppTransactionId = ocppTransactionId,
                StartedAt = request.Timestamp, // Already UTC from JSON deserializer
                Status = ChargingSessionStatus.Charging,
                AuthorizationMethodId = authMethod.Id
            };

            context.ChargingSessions.Add(session);
            
            _logger.LogInformation("Transaction started: OcppTransactionId={TransactionId}, User={UserId}, Station={StationId}", 
                ocppTransactionId, authMethod.UserId, station.Id);
        }

        // Update connector status
        connector.Status = ConnectorStatus.Occupied;
        
        await context.SaveChangesAsync();

        return new StartTransactionResponse
        {
            IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Accepted },
            TransactionId = ocppTransactionId
        };
    }

    private async Task<StopTransactionResponse> HandleStopTransactionAsync(string chargeBoxId, StopTransactionRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Find session by OCPP transaction ID
        var session = await context.ChargingSessions
            .Include(s => s.ChargingConnector)
                .ThenInclude(c => c.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
            .FirstOrDefaultAsync(s => s.OcppTransactionId == request.TransactionId);

        if (session == null)
        {
            _logger.LogWarning("Session not found for TransactionId: {TransactionId}", request.TransactionId);
            return new StopTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid }
            };
        }

        // Update session
        session.ChargingCompletedAt = request.Timestamp; // Zeitpunkt der Energielieferung-Ende
        session.EndedAt = request.Timestamp; // Zunächst gleichgesetzt, wird später aktualisiert wenn Stecker gezogen
        var duration = (session.EndedAt.Value - session.StartedAt).TotalHours;
        session.EnergyDelivered = (decimal)(request.MeterStop / 1000.0); // Convert Wh to kWh
        session.Status = ChargingSessionStatus.Completed;

        // Calculate cost using tariff system (berücksichtigt jetzt ChargingCompletedAt für Standzeit)
        session.Cost = await CalculateSessionCostAsync(context, session);

        await context.SaveChangesAsync();

        _logger.LogInformation("Transaction stopped: SessionId={SessionId}, Energy={Energy}kWh, Cost={Cost}EUR", 
            session.Id, session.EnergyDelivered, session.Cost);

        // Create billing transaction automatically (only if not already created by Web-UI stop)
        var existingTransaction = await context.BillingTransactions
            .AnyAsync(bt => bt.ChargingSessionId == session.Id);
        
        if (!existingTransaction)
        {
            await CreateBillingTransactionAsync(session);
        }
        else
        {
            _logger.LogInformation("Billing transaction already exists for session {SessionId}, skipping creation", session.Id);
        }

        return new StopTransactionResponse
        {
            IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Accepted }
        };
    }

    private async Task<MeterValuesResponse> HandleMeterValuesAsync(string chargeBoxId, MeterValuesRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        if (request.TransactionId.HasValue)
        {
            var session = await context.ChargingSessions
                .FirstOrDefaultAsync(s => s.OcppTransactionId == request.TransactionId.Value);

            if (session != null && request.MeterValue.Any())
            {
                // Update current meter value (take first meter reading)
                var firstMeterValue = request.MeterValue.First();
                var energySample = firstMeterValue.SampledValue.FirstOrDefault(s => 
                    s.Measurand == "Energy.Active.Import.Register" || string.IsNullOrEmpty(s.Measurand));

                if (energySample != null && double.TryParse(energySample.Value, out var meterValue))
                {
                    var energyConsumed = (decimal)(meterValue / 1000.0); // Convert to kWh
                    session.EnergyDelivered = energyConsumed;
                    
                    // Update cost based on current energy consumed
                    session.Cost = await CalculateSessionCostAsync(context, session);
                    
                    await context.SaveChangesAsync();
                }
            }
        }

        _logger.LogDebug("MeterValues from {ChargeBoxId}: Connector {ConnectorId}", 
            chargeBoxId, request.ConnectorId);

        return new MeterValuesResponse();
    }

    /// <summary>
    /// Calculate session cost using tariff system
    /// </summary>
    private async Task<decimal> CalculateSessionCostAsync(ApplicationDbContext context, ChargingSession session)
    {
        if (!session.UserId.HasValue)
        {
            // No user - use default rate
            return session.EnergyDelivered * 0.30m;
        }

        try
        {
            // Get applicable tariff for user
            var userGroupIds = await context.UserGroupMemberships
                .Where(m => m.UserId == session.UserId.Value)
                .Select(m => m.UserGroupId)
                .ToListAsync();

            Tariff? tariff = null;

            if (userGroupIds.Any())
            {
                // Get tariff from user groups
                var userGroupTariff = await context.UserGroupTariffs
                    .Include(ugt => ugt.Tariff)
                        .ThenInclude(t => t.Components)
                    .Where(ugt => userGroupIds.Contains(ugt.UserGroupId))
                    .Where(ugt => ugt.Tariff.IsActive)
                    .Where(ugt => (ugt.Tariff.ValidFrom == null || ugt.Tariff.ValidFrom <= DateTime.UtcNow))
                    .Where(ugt => (ugt.Tariff.ValidUntil == null || ugt.Tariff.ValidUntil >= DateTime.UtcNow))
                    .OrderByDescending(ugt => ugt.Priority)
                    .FirstOrDefaultAsync();

                tariff = userGroupTariff?.Tariff;
            }

            if (tariff == null)
            {
                // Try default tariff for tenant
                var user = await context.Users.FindAsync(session.UserId.Value);
                if (user != null)
                {
                    tariff = await context.Tariffs
                        .Include(t => t.Components)
                        .Where(t => t.TenantId == user.TenantId)
                        .Where(t => t.IsDefault && t.IsActive)
                        .Where(t => (t.ValidFrom == null || t.ValidFrom <= DateTime.UtcNow))
                        .Where(t => (t.ValidUntil == null || t.ValidUntil >= DateTime.UtcNow))
                        .FirstOrDefaultAsync();
                }
            }

            if (tariff == null)
            {
                // No tariff found - use default rate
                _logger.LogWarning("No tariff found for user {UserId}, using default rate", session.UserId.Value);
                return session.EnergyDelivered * 0.30m;
            }

            // Calculate cost based on tariff components
            decimal totalCost = 0m;
            var sessionEnd = session.EndedAt ?? DateTime.UtcNow;
            var chargingEnd = session.ChargingCompletedAt ?? sessionEnd; // Falls nicht gesetzt, verwende EndedAt
            
            // Ladezeit = Von Start bis Ende der Energielieferung
            var chargingDurationMinutes = (int)(chargingEnd - session.StartedAt).TotalMinutes;
            
            // Standzeit = Von Ende der Energielieferung bis Connector-Freigabe
            var idleTimeMinutes = (int)(sessionEnd - chargingEnd).TotalMinutes;
            
            // Gesamte Parkzeit = Gesamte Session-Dauer
            var totalParkingMinutes = (int)(sessionEnd - session.StartedAt).TotalMinutes;

            foreach (var component in tariff.Components.Where(c => c.IsActive))
            {
                decimal componentCost = component.Type switch
                {
                    TariffComponentType.Energy => session.EnergyDelivered * component.Price,
                    TariffComponentType.ChargingTime => chargingDurationMinutes * component.Price,
                    TariffComponentType.ParkingTime => totalParkingMinutes * component.Price, // Gesamte Zeit inkl. Laden
                    TariffComponentType.IdleTime => idleTimeMinutes * component.Price, // Nur Standzeit NACH Ladeende
                    TariffComponentType.SessionFee => component.Price,
                    _ => 0m
                };

                // Apply grace period for time-based charges
                if (component.Type == TariffComponentType.ChargingTime)
                {
                    if (component.GracePeriodMinutes.HasValue)
                    {
                        var billableMinutes = Math.Max(0, chargingDurationMinutes - component.GracePeriodMinutes.Value);
                        componentCost = billableMinutes * component.Price;
                    }
                }
                else if (component.Type == TariffComponentType.IdleTime)
                {
                    // Für Standzeit: Grace Period NUR auf die Standzeit anwenden
                    if (component.GracePeriodMinutes.HasValue)
                    {
                        var billableMinutes = Math.Max(0, idleTimeMinutes - component.GracePeriodMinutes.Value);
                        componentCost = billableMinutes * component.Price;
                    }
                }
                else if (component.Type == TariffComponentType.ParkingTime)
                {
                    if (component.GracePeriodMinutes.HasValue)
                    {
                        var billableMinutes = Math.Max(0, totalParkingMinutes - component.GracePeriodMinutes.Value);
                        componentCost = billableMinutes * component.Price;
                    }
                }

                // Apply min/max charges
                if (component.MinimumCharge.HasValue && componentCost < component.MinimumCharge.Value)
                    componentCost = component.MinimumCharge.Value;

                if (component.MaximumCharge.HasValue && componentCost > component.MaximumCharge.Value)
                    componentCost = component.MaximumCharge.Value;

                totalCost += componentCost;
            }

            _logger.LogInformation("Calculated cost for session {SessionId}: {Cost} {Currency} (Tariff: {TariffName})",
                session.Id, totalCost, tariff.Currency, tariff.Name);

            return totalCost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating session cost for session {SessionId}, using default rate", session.Id);
            return session.EnergyDelivered * 0.30m;
        }
    }

    /// <summary>
    /// Check if a user has access to a charging station via UserGroup permissions
    /// </summary>
    private async Task<bool> CheckUserStationAccessAsync(ApplicationDbContext context, Guid userId, Guid stationId)
    {
        // Get all user groups the user belongs to
        var userGroupIds = await context.UserGroupMemberships
            .Where(m => m.UserId == userId)
            .Select(m => m.UserGroupId)
            .ToListAsync();

        if (!userGroupIds.Any())
        {
            _logger.LogDebug("User {UserId} is not member of any user groups", userId);
            return false;
        }

        // Get all charging station groups this station belongs to
        var stationGroupIds = await context.ChargingStationGroupMemberships
            .Where(m => m.ChargingStationId == stationId)
            .Select(m => m.ChargingStationGroupId)
            .ToListAsync();

        if (!stationGroupIds.Any())
        {
            _logger.LogDebug("Station {StationId} is not member of any charging station groups", stationId);
            return false;
        }

        // Check if any of the user's groups has permission for any of the station's groups
        var hasPermission = await context.UserGroupChargingStationGroupPermissions
            .AnyAsync(p => userGroupIds.Contains(p.UserGroupId) && stationGroupIds.Contains(p.ChargingStationGroupId));

        if (hasPermission)
        {
            _logger.LogInformation("User {UserId} has access to station {StationId} via user group permissions", 
                userId, stationId);
        }
        else
        {
            _logger.LogWarning("User {UserId} has NO access to station {StationId} - no matching permissions found", 
                userId, stationId);
        }

        return hasPermission;
    }

    /// <summary>
    /// Creates a billing transaction for a completed charging session
    /// </summary>
    private async Task CreateBillingTransactionAsync(ChargingSession session)
    {
        if (_serviceProviderFactory == null)
        {
            _logger.LogWarning("ServiceProviderFactory not available, skipping billing transaction creation");
            return;
        }

        try
        {
            using var scope = _serviceProviderFactory().CreateScope();
            
            // Use reflection to get IBillingService to avoid hard dependency on Api project
            var billingServiceType = Type.GetType("ChargingControlSystem.Api.Services.IBillingService, ChargingControlSystem.Api");
            if (billingServiceType == null)
            {
                _logger.LogWarning("IBillingService type not found, skipping billing transaction creation");
                return;
            }

            var billingService = scope.ServiceProvider.GetService(billingServiceType);
            if (billingService == null)
            {
                _logger.LogWarning("IBillingService not available, skipping billing transaction creation");
                return;
            }

            // Call CreateTransactionForSessionAsync via reflection
            var method = billingServiceType.GetMethod("CreateTransactionForSessionAsync");
            if (method != null)
            {
                var task = method.Invoke(billingService, new object[] { session });
                if (task is Task<BillingTransaction> transactionTask)
                {
                    var transaction = await transactionTask;
                    _logger.LogInformation("Created billing transaction {TransactionId} for session {SessionId}", 
                        transaction.Id, session.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create billing transaction for session {SessionId}", session.Id);
            // Don't throw - billing should not block the OCPP response
        }
    }

    /// <summary>
    /// Notify clients about station status change (via SignalR)
    /// </summary>
    private async Task NotifyStationStatusChangedAsync(Guid tenantId, Guid stationId, string status, string? message = null)
    {
        if (_serviceProviderFactory == null)
        {
            return;
        }

        try
        {
            using var scope = _serviceProviderFactory().CreateScope();
            
            // Use reflection to get INotificationService to avoid hard dependency on Api project
            var notificationServiceType = Type.GetType("ChargingControlSystem.Api.Services.INotificationService, ChargingControlSystem.Api");
            if (notificationServiceType == null)
            {
                return;
            }

            var notificationService = scope.ServiceProvider.GetService(notificationServiceType);
            if (notificationService == null)
            {
                return;
            }

            // Call NotifyStationStatusChangedAsync via reflection
            var method = notificationServiceType.GetMethod("NotifyStationStatusChangedAsync");
            if (method != null)
            {
                var task = method.Invoke(notificationService, new object[] { tenantId, stationId, status, message });
                if (task is Task notifyTask)
                {
                    await notifyTask;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send station status notification for station {StationId}", stationId);
            // Don't throw - notification should not block the OCPP response
        }
    }

    /// <summary>
    /// Notify clients about connector status change (via SignalR)
    /// </summary>
    private async Task NotifyConnectorStatusChangedAsync(Guid tenantId, Guid connectorId, string status, string? message = null)
    {
        if (_serviceProviderFactory == null)
        {
            return;
        }

        try
        {
            using var scope = _serviceProviderFactory().CreateScope();
            
            // Use reflection to get INotificationService to avoid hard dependency on Api project
            var notificationServiceType = Type.GetType("ChargingControlSystem.Api.Services.INotificationService, ChargingControlSystem.Api");
            if (notificationServiceType == null)
            {
                return;
            }

            var notificationService = scope.ServiceProvider.GetService(notificationServiceType);
            if (notificationService == null)
            {
                return;
            }

            // Call NotifyConnectorStatusChangedAsync via reflection
            var method = notificationServiceType.GetMethod("NotifyConnectorStatusChangedAsync");
            if (method != null)
            {
                var task = method.Invoke(notificationService, new object[] { tenantId, connectorId, status, message });
                if (task is Task notifyTask)
                {
                    await notifyTask;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send connector status notification for connector {ConnectorId}", connectorId);
            // Don't throw - notification should not block the OCPP response
        }
    }
}
