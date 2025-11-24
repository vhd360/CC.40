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
            "FirmwareStatusNotification" => await HandleFirmwareStatusNotificationAsync(chargeBoxId, payload.ToObject<FirmwareStatusNotificationRequest>(serializer)!),
            "GetConfiguration" => await HandleGetConfigurationAsync(chargeBoxId, payload.ToObject<GetConfigurationRequest>(serializer)!),
            "ChangeConfiguration" => await HandleChangeConfigurationAsync(chargeBoxId, payload.ToObject<ChangeConfigurationRequest>(serializer)!),
            "GetDiagnostics" => await HandleGetDiagnosticsAsync(chargeBoxId, payload.ToObject<GetDiagnosticsRequest>(serializer)!),
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
            .Include(s => s.ChargingPark)
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

        // Update station information from BootNotification
        var previousStatus = station.Status;
        station.Vendor = request.ChargePointVendor;
        station.Model = request.ChargePointModel;
        
        // Additional BootNotification fields
        station.SerialNumber = request.ChargePointSerialNumber ?? request.ChargeBoxSerialNumber;
        station.FirmwareVersion = request.FirmwareVersion;
        station.Iccid = request.Iccid;
        station.Imsi = request.Imsi;
        station.MeterType = request.MeterType;
        station.MeterSerialNumber = request.MeterSerialNumber;
        
        station.LastHeartbeat = DateTime.UtcNow;
        station.Status = ChargingStationStatus.Available;

        await context.SaveChangesAsync();

        // Notify clients if station was previously unavailable and is now back online
        // Oder wenn Status sich geändert hat (für Frontend-Synchronisation)
        if (previousStatus != ChargingStationStatus.Available)
        {
            _logger.LogInformation("BootNotification: Station {ChargeBoxId} (ID: {StationId}) changed from {PreviousStatus} to Available, sending notification", 
                chargeBoxId, station.Id, previousStatus);
            await NotifyStationStatusChangedAsync(station.ChargingPark.TenantId, station.Id, "Available", "Station is back online");
        }
        else
        {
            // Auch wenn Status bereits Available ist, sende Benachrichtigung für Frontend-Synchronisation
            _logger.LogInformation("BootNotification: Station {ChargeBoxId} (ID: {StationId}) is Available, sending notification for sync", 
                chargeBoxId, station.Id);
            await NotifyStationStatusChangedAsync(station.ChargingPark.TenantId, station.Id, "Available", "Station online");
        }

        _logger.LogInformation("BootNotification from {ChargeBoxId}: {Vendor} {Model} (FW: {FirmwareVersion}, Serial: {SerialNumber})", 
            chargeBoxId, request.ChargePointVendor, request.ChargePointModel, 
            request.FirmwareVersion ?? "N/A", station.SerialNumber ?? "N/A");

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
            var lastHeartbeatBeforeUpdate = station.LastHeartbeat;
            
            _logger.LogInformation("Heartbeat from {ChargeBoxId}: Station {StationId} ({StationName}), LastHeartbeat before: {LastHeartbeatBefore}", 
                chargeBoxId, station.Id, station.Name, lastHeartbeatBeforeUpdate);
            
            // Update heartbeat
            station.LastHeartbeat = DateTime.UtcNow;
            _logger.LogInformation("Set LastHeartbeat to: {NewHeartbeat}", station.LastHeartbeat);
            
            // If station was unavailable and now sending heartbeat, mark as available
            if (previousStatus == ChargingStationStatus.Unavailable)
            {
                station.Status = ChargingStationStatus.Available;
                await context.SaveChangesAsync();
                
                // Notify clients about status change
                _logger.LogInformation("Station {ChargeBoxId} (ID: {StationId}) changed from Unavailable to Available, sending notification", 
                    chargeBoxId, station.Id);
                await NotifyStationStatusChangedAsync(station.ChargingPark.TenantId, station.Id, "Available", "Station is back online");
            }
            else
            {
                // Wenn Status bereits Available ist, sende trotzdem Benachrichtigung für Frontend-Synchronisation
                // Sende alle 30 Sekunden eine Benachrichtigung, um Frontend zu synchronisieren
                var timeSinceLastHeartbeat = lastHeartbeatBeforeUpdate.HasValue 
                    ? (DateTime.UtcNow - lastHeartbeatBeforeUpdate.Value).TotalSeconds 
                    : 999; // Wenn kein vorheriger Heartbeat, sende sofort
                
                if (timeSinceLastHeartbeat >= 30)
                {
                    if (previousStatus != ChargingStationStatus.Available)
                    {
                        station.Status = ChargingStationStatus.Available;
                    }
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Station {ChargeBoxId} (ID: {StationId}) sending periodic status notification (Available) for sync - {TimeSinceLastHeartbeat}s since last heartbeat", 
                        chargeBoxId, station.Id, timeSinceLastHeartbeat);
                    await NotifyStationStatusChangedAsync(station.ChargingPark.TenantId, station.Id, "Available", "Station online");
                }
                else
                {
                    if (previousStatus != ChargingStationStatus.Available)
                    {
                        station.Status = ChargingStationStatus.Available;
                        await context.SaveChangesAsync();
                        _logger.LogInformation("Station {ChargeBoxId} (ID: {StationId}) status changed to Available, sending notification", 
                            chargeBoxId, station.Id);
                        await NotifyStationStatusChangedAsync(station.ChargingPark.TenantId, station.Id, "Available", "Station status updated");
                    }
                    else
                    {
                        await context.SaveChangesAsync();
                        _logger.LogInformation("Station {ChargeBoxId} (ID: {StationId}) heartbeat saved to DB, status already Available ({TimeSinceLastHeartbeat}s since last heartbeat)", 
                            chargeBoxId, station.Id, timeSinceLastHeartbeat);
                    }
                }
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

            // Update charging point status if ConnectorId > 0
            if (request.ConnectorId > 0)
            {
                var chargingPoint = station.ChargingPoints.FirstOrDefault(cp => cp.EvseId == request.ConnectorId);
                if (chargingPoint != null)
                {
                    var previousStatus = chargingPoint.Status;
                    
                    chargingPoint.Status = request.Status switch
                    {
                        ChargePointStatus.Available => ChargingPointStatus.Available,
                        ChargePointStatus.Preparing => ChargingPointStatus.Preparing,
                        ChargePointStatus.Charging => ChargingPointStatus.Charging,
                        ChargePointStatus.Unavailable => ChargingPointStatus.Unavailable,
                        ChargePointStatus.Faulted => ChargingPointStatus.Faulted,
                        ChargePointStatus.Reserved => ChargingPointStatus.Reserved,
                        ChargePointStatus.Finishing => ChargingPointStatus.Finishing,
                        _ => ChargingPointStatus.Unavailable
                    };

                    // Notify about charging point status change if it changed
                    if (previousStatus != chargingPoint.Status)
                    {
                        chargingPoint.LastStatusChange = DateTime.UtcNow;
                        await NotifyConnectorStatusChangedAsync(
                            station.ChargingPark.TenantId, 
                            chargingPoint.Id, 
                            chargingPoint.Status.ToString(), 
                            request.ErrorCode);
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

    private async Task<FirmwareStatusNotificationResponse> HandleFirmwareStatusNotificationAsync(string chargeBoxId, FirmwareStatusNotificationRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var station = await context.ChargingStations
            .Include(s => s.ChargingPark)
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station != null)
        {
            // Update firmware status in station
            station.FirmwareStatus = request.Status;
            station.LastFirmwareStatusUpdate = DateTime.UtcNow;

            // Create firmware history entry
            var firmwareHistory = new ChargingStationFirmwareHistory
            {
                Id = Guid.NewGuid(),
                ChargingStationId = station.Id,
                FirmwareVersion = station.FirmwareVersion ?? "Unknown",
                Status = request.Status,
                Info = request.Info,
                Timestamp = request.Timestamp ?? DateTime.UtcNow
            };

            context.ChargingStationFirmwareHistory.Add(firmwareHistory);

            await context.SaveChangesAsync();

            // Log firmware status update
            _logger.LogInformation("FirmwareStatusNotification from {ChargeBoxId}: Status={Status}, Info={Info}, Version={Version}", 
                chargeBoxId, request.Status, request.Info ?? "N/A", station.FirmwareVersion ?? "Unknown");

            // Send notification for critical firmware status changes
            if (request.Status == FirmwareStatus.InstallationFailed || request.Status == FirmwareStatus.DownloadFailed)
            {
                await NotifyStationStatusChangedAsync(
                    station.ChargingPark.TenantId, 
                    station.Id, 
                    "FirmwareUpdateFailed", 
                    $"Firmware update failed: {request.Status} - {request.Info ?? "No details"}");
            }
            else if (request.Status == FirmwareStatus.Installed)
            {
                await NotifyStationStatusChangedAsync(
                    station.ChargingPark.TenantId, 
                    station.Id, 
                    "FirmwareUpdateCompleted", 
                    $"Firmware update completed successfully: {station.FirmwareVersion ?? "Unknown"}");
            }
        }
        else
        {
            _logger.LogWarning("Received FirmwareStatusNotification from unknown ChargeBox: {ChargeBoxId}", chargeBoxId);
        }

        return new FirmwareStatusNotificationResponse();
    }

    private async Task<GetConfigurationResponse> HandleGetConfigurationAsync(string chargeBoxId, GetConfigurationRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var station = await context.ChargingStations
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station == null)
        {
            _logger.LogWarning("GetConfiguration from unknown ChargeBox: {ChargeBoxId}", chargeBoxId);
            return new GetConfigurationResponse
            {
                ConfigurationKey = new List<ConfigurationKey>(),
                UnknownKey = request.Key
            };
        }

        // If configuration is stored, deserialize it
        var configurationKeys = new List<ConfigurationKey>();
        if (!string.IsNullOrEmpty(station.ConfigurationJson))
        {
            try
            {
                var storedConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ConfigurationKey>>(station.ConfigurationJson);
                if (storedConfig != null)
                {
                    configurationKeys = storedConfig;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize configuration for station {ChargeBoxId}", chargeBoxId);
            }
        }

        // If specific keys requested, filter them
        if (request.Key != null && request.Key.Any())
        {
            var requestedKeys = request.Key.ToHashSet();
            var foundKeys = configurationKeys.Where(k => requestedKeys.Contains(k.Key)).ToList();
            var unknownKeys = requestedKeys.Where(k => !configurationKeys.Any(c => c.Key == k)).ToList();

            return new GetConfigurationResponse
            {
                ConfigurationKey = foundKeys,
                UnknownKey = unknownKeys.Any() ? unknownKeys : null
            };
        }

        // Return all configuration keys
        return new GetConfigurationResponse
        {
            ConfigurationKey = configurationKeys,
            UnknownKey = null
        };
    }

    private async Task<ChangeConfigurationResponse> HandleChangeConfigurationAsync(string chargeBoxId, ChangeConfigurationRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var station = await context.ChargingStations
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station == null)
        {
            _logger.LogWarning("ChangeConfiguration from unknown ChargeBox: {ChargeBoxId}", chargeBoxId);
            return new ChangeConfigurationResponse
            {
                Status = ConfigurationStatus.Rejected
            };
        }

        // Load current configuration
        var configurationKeys = new List<ConfigurationKey>();
        if (!string.IsNullOrEmpty(station.ConfigurationJson))
        {
            try
            {
                var storedConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ConfigurationKey>>(station.ConfigurationJson);
                if (storedConfig != null)
                {
                    configurationKeys = storedConfig;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize configuration for station {ChargeBoxId}", chargeBoxId);
            }
        }

        // Check if key exists and is readonly
        var existingKey = configurationKeys.FirstOrDefault(k => k.Key == request.Key);
        if (existingKey != null && existingKey.Readonly)
        {
            _logger.LogWarning("Attempted to change readonly configuration key {Key} on station {ChargeBoxId}", request.Key, chargeBoxId);
            return new ChangeConfigurationResponse
            {
                Status = ConfigurationStatus.Rejected
            };
        }

        // Update or add configuration key
        if (existingKey != null)
        {
            existingKey.Value = request.Value;
        }
        else
        {
            configurationKeys.Add(new ConfigurationKey
            {
                Key = request.Key,
                Value = request.Value,
                Readonly = false
            });
        }

        // Save configuration
        station.ConfigurationJson = Newtonsoft.Json.JsonConvert.SerializeObject(configurationKeys);
        station.LastConfigurationUpdate = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation("ChangeConfiguration on {ChargeBoxId}: Key={Key}, Value={Value}", 
            chargeBoxId, request.Key, request.Value);

        return new ChangeConfigurationResponse
        {
            Status = ConfigurationStatus.Accepted
        };
    }

    private async Task<GetDiagnosticsResponse> HandleGetDiagnosticsAsync(string chargeBoxId, GetDiagnosticsRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var station = await context.ChargingStations
            .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

        if (station == null)
        {
            _logger.LogWarning("GetDiagnostics from unknown ChargeBox: {ChargeBoxId}", chargeBoxId);
            return new GetDiagnosticsResponse();
        }

        // Create diagnostics request record
        var diagnostics = new ChargingStationDiagnostics
        {
            Id = Guid.NewGuid(),
            ChargingStationId = station.Id,
            RequestedAt = DateTime.UtcNow,
            Status = DiagnosticsStatus.Pending,
            StartTime = request.StartTime,
            StopTime = request.StopTime
        };

        context.ChargingStationDiagnostics.Add(diagnostics);
        await context.SaveChangesAsync();

        _logger.LogInformation("GetDiagnostics requested for {ChargeBoxId}: Location={Location}, StartTime={StartTime}, StopTime={StopTime}", 
            chargeBoxId, request.Location, request.StartTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A", 
            request.StopTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A");

        // Note: The actual diagnostics file will be uploaded by the charge point to the specified location
        // This handler just records the request. The file upload would be handled separately (e.g., via HTTP endpoint)

        return new GetDiagnosticsResponse
        {
            FileName = null // File will be available after charge point uploads it
        };
    }

    private async Task<AuthorizeResponse> HandleAuthorizeAsync(string chargeBoxId, AuthorizeRequest request)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Check if the IdTag (RFID) exists in authorization methods
        // Some charging stations may strip the "WEB_" prefix from the IdTag, so we need to search for both variants
        var authMethod = await context.AuthorizationMethods
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => 
                (a.Identifier == request.IdTag || 
                 a.Identifier == $"WEB_{request.IdTag}" ||
                 (a.Identifier.StartsWith("WEB_") && a.Identifier.Length > 4 && a.Identifier.Substring(4) == request.IdTag)) &&
                a.Type == AuthorizationMethodType.RFID && 
                a.IsActive);

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
        _logger.LogInformation("HandleStartTransactionAsync: ChargeBoxId={ChargeBoxId}, ConnectorId={ConnectorId}, IdTag={IdTag}", 
            chargeBoxId, request.ConnectorId, request.IdTag);
        
        using var context = await _contextFactory.CreateDbContextAsync();

        // Find station with charging points
        var station = await context.ChargingStations
            .Include(s => s.ChargingPoints)
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

        // Find charging point first to check for existing session
        var chargingPoint = station.ChargingPoints.FirstOrDefault(cp => cp.EvseId == request.ConnectorId);
        
        // Check if there's already a session for this charging point (from Web-UI RemoteStart)
        // This allows us to accept StartTransaction even if authorization fails, if a session already exists
        ChargingSession? existingSessionEarly = null;
        if (chargingPoint != null)
        {
            _logger.LogInformation("Looking for existing session for ChargingPointId={ChargingPointId}, EvseId={EvseId}", 
                chargingPoint.Id, chargingPoint.EvseId);
            
            existingSessionEarly = await context.ChargingSessions
                .Include(s => s.AuthorizationMethod)
                .FirstOrDefaultAsync(s => 
                    s.ChargingPointId == chargingPoint.Id && 
                    s.Status == ChargingSessionStatus.Charging &&
                    s.OcppTransactionId == null);
            
            if (existingSessionEarly != null)
            {
                _logger.LogInformation("Found existing session {SessionId} for ChargingPointId={ChargingPointId}", 
                    existingSessionEarly.Id, chargingPoint.Id);
            }
            else
            {
                _logger.LogWarning("No existing session found for ChargingPointId={ChargingPointId}, EvseId={EvseId}. Checking all sessions for this point...", 
                    chargingPoint.Id, chargingPoint.EvseId);
                
                // Debug: List all sessions for this charging point
                var allSessions = await context.ChargingSessions
                    .Where(s => s.ChargingPointId == chargingPoint.Id)
                    .Select(s => new { s.Id, s.Status, s.OcppTransactionId, s.StartedAt })
                    .ToListAsync();
                _logger.LogWarning("All sessions for ChargingPointId={ChargingPointId}: {Sessions}", 
                    chargingPoint.Id, string.Join(", ", allSessions.Select(s => $"Id={s.Id}, Status={s.Status}, OcppTransactionId={s.OcppTransactionId}")));
            }
        }
        else
        {
            _logger.LogWarning("ChargingPoint not found for ConnectorId={ConnectorId} on station {StationId}", 
                request.ConnectorId, station.Id);
        }
        
        // Find authorization method
        // Some charging stations may strip the "WEB_" prefix from the IdTag, so we need to search for both variants
        var authMethod = await context.AuthorizationMethods
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => 
                (a.Identifier == request.IdTag || 
                 a.Identifier == $"WEB_{request.IdTag}" ||
                 (a.Identifier.StartsWith("WEB_") && a.Identifier.Length > 4 && a.Identifier.Substring(4) == request.IdTag)) &&
                a.Type == AuthorizationMethodType.RFID &&
                a.IsActive);

        // If authorization fails but we have an existing session, accept it anyway
        if (authMethod == null)
        {
            if (existingSessionEarly != null)
            {
                _logger.LogWarning("Authorization method not found for IdTag: {IdTag}, but existing session {SessionId} found. Accepting StartTransaction.", 
                    request.IdTag, existingSessionEarly.Id);
                // Use the authorization method from the existing session
                if (existingSessionEarly.AuthorizationMethod != null)
                {
                    authMethod = existingSessionEarly.AuthorizationMethod;
                }
                else
                {
                    _logger.LogWarning("Existing session {SessionId} has no AuthorizationMethod, cannot proceed", existingSessionEarly.Id);
                    return new StartTransactionResponse
                    {
                        IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                        TransactionId = 0
                    };
                }
            }
            else
            {
                _logger.LogWarning("Authorization method not found for IdTag: {IdTag}. Searched for exact match, WEB_{IdTag}, and WEB_*{IdTag}", 
                    request.IdTag);
                
                // Debug: List all authorization methods for this IdTag pattern
                var allAuthMethods = await context.AuthorizationMethods
                    .Where(a => a.Type == AuthorizationMethodType.RFID)
                    .Select(a => a.Identifier)
                    .ToListAsync();
                _logger.LogWarning("Available RFID authorization methods: {Methods}", string.Join(", ", allAuthMethods));
                
                return new StartTransactionResponse
                {
                    IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                    TransactionId = 0
                };
            }
        }
        
        if (authMethod.User == null || !authMethod.User.IsActive)
        {
            // If we have an existing session, accept it anyway
            if (existingSessionEarly != null)
            {
                _logger.LogWarning("User not active for IdTag: {IdTag}, but existing session {SessionId} found. Accepting StartTransaction.", 
                    request.IdTag, existingSessionEarly.Id);
            }
            else
            {
                _logger.LogWarning("User not found or inactive for IdTag: {IdTag}, UserId: {UserId}", 
                    request.IdTag, authMethod.UserId);
                return new StartTransactionResponse
                {
                    IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                    TransactionId = 0
                };
            }
        }
        
        _logger.LogInformation("Authorization method found: IdTag={IdTag}, StoredIdentifier={StoredIdentifier}, UserId={UserId}", 
            request.IdTag, authMethod.Identifier, authMethod.UserId);

        // Check if user has access to this station via UserGroup permissions
        // If we have an existing session, skip this check (session was already authorized)
        if (existingSessionEarly == null)
        {
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
        }
        else
        {
            _logger.LogInformation("Skipping user access check for existing session {SessionId}", existingSessionEarly.Id);
        }

        // Use charging point found earlier, or find it now if not found
        if (chargingPoint == null)
        {
            chargingPoint = station.ChargingPoints.FirstOrDefault(cp => cp.EvseId == request.ConnectorId);
        }
        
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

        // Check if charging point is available (ChargingPoint is now the connector)
        // Accept Preparing, Available, and Occupied statuses - Preparing is valid during startup
        if (chargingPoint.Status != ChargingPointStatus.Available && 
            chargingPoint.Status != ChargingPointStatus.Occupied &&
            chargingPoint.Status != ChargingPointStatus.Preparing)
        {
            _logger.LogWarning("ChargingPoint {ChargingPointId} (EvseId: {EvseId}) is not available (Status: {Status})", 
                chargingPoint.Id, chargingPoint.EvseId, chargingPoint.Status);
            return new StartTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid },
                TransactionId = 0
            };
        }
        
        _logger.LogInformation("ChargingPoint {ChargingPointId} (EvseId: {EvseId}) status is {Status}, accepting StartTransaction", 
            chargingPoint.Id, chargingPoint.EvseId, chargingPoint.Status);

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

        // Use existing session found earlier, or check again with the generated transaction ID
        ChargingSession? existingSession = existingSessionEarly;
        
        // If we didn't find a session earlier, try to find by transaction ID
        if (existingSession == null && ocppTransactionId > 0)
        {
            existingSession = await context.ChargingSessions
                .FirstOrDefaultAsync(s => s.OcppTransactionId == ocppTransactionId);
        }
        
        // Fallback: Find by charging point and status if no transaction ID match
        if (existingSession == null && chargingPoint != null)
        {
            existingSession = await context.ChargingSessions
                .FirstOrDefaultAsync(s => 
                    s.ChargingPointId == chargingPoint.Id && 
                    s.Status == ChargingSessionStatus.Charging &&
                    s.AuthorizationMethodId == authMethod.Id &&
                    s.OcppTransactionId == null);
        }

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
                ChargingPointId = chargingPoint.Id,
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

        // Update charging point status
        chargingPoint.Status = ChargingPointStatus.Occupied;
        chargingPoint.LastStatusChange = DateTime.UtcNow;
        
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
            .Include(s => s.ChargingPoint)
                .ThenInclude(cp => cp.ChargingStation)
                    .ThenInclude(cs => cs.ChargingPark)
            .FirstOrDefaultAsync(s => s.OcppTransactionId == request.TransactionId);

        if (session == null)
        {
            _logger.LogWarning("Session not found for TransactionId: {TransactionId}", request.TransactionId);
            return new StopTransactionResponse
            {
                IdTagInfo = new IdTagInfo { Status = AuthorizationStatus.Invalid }
            };
        }

        var chargingPoint = session.ChargingPoint;
        var tenantId = session.TenantId;

        // Update session
        session.ChargingCompletedAt = request.Timestamp; // Zeitpunkt der Energielieferung-Ende
        session.EndedAt = request.Timestamp; // Zunächst gleichgesetzt, wird später aktualisiert wenn Stecker gezogen
        var duration = (session.EndedAt.Value - session.StartedAt).TotalHours;
        session.EnergyDelivered = (decimal)(request.MeterStop / 1000.0); // Convert Wh to kWh
        session.Status = ChargingSessionStatus.Completed;

        // Calculate cost using tariff system (berücksichtigt jetzt ChargingCompletedAt für Standzeit)
        session.Cost = await CalculateSessionCostAsync(context, session);

        // Update charging point status to Available
        if (chargingPoint != null)
        {
            chargingPoint.Status = ChargingPointStatus.Available;
            chargingPoint.LastStatusChange = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        _logger.LogInformation("Transaction stopped: SessionId={SessionId}, Energy={Energy}kWh, Cost={Cost}EUR", 
            session.Id, session.EnergyDelivered, session.Cost);

        // Send SignalR notifications
        if (session.UserId.HasValue && chargingPoint != null)
        {
            // Notify session update
            await NotifySessionUpdateAsync(tenantId, session.UserId.Value, session.Id, "Completed", "Ladevorgang beendet");
            
            // Notify connector status change
            await NotifyConnectorStatusChangedAsync(tenantId, chargingPoint.Id, "Available", "Ladepunkt wieder verfügbar");
        }

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

        ChargingSession? session = null;

        if (request.TransactionId.HasValue)
        {
            // Try to find session by OCPP Transaction ID
            session = await context.ChargingSessions
                .Include(s => s.ChargingPoint)
                    .ThenInclude(cp => cp.ChargingStation)
                .FirstOrDefaultAsync(s => s.OcppTransactionId == request.TransactionId.Value);
        }

        // Fallback: If no session found by TransactionId, try to find active session for this connector
        if (session == null && request.ConnectorId > 0)
        {
            var station = await context.ChargingStations
                .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);
            
            if (station != null)
            {
                var chargingPoint = await context.ChargingPoints
                    .FirstOrDefaultAsync(cp => cp.ChargingStationId == station.Id && cp.EvseId == request.ConnectorId);
                
                if (chargingPoint != null)
                {
                    // Find active charging session for this charging point
                    session = await context.ChargingSessions
                        .Where(s => s.ChargingPointId == chargingPoint.Id && 
                                   s.Status == ChargingSessionStatus.Charging)
                        .OrderByDescending(s => s.StartedAt)
                        .FirstOrDefaultAsync();
                    
                    // If we found a session but it has no OcppTransactionId, and MeterValues has a TransactionId > 0,
                    // update the session with the TransactionId from MeterValues
                    // This handles the case where StartTransaction was rejected but charging started anyway
                    if (session != null && !session.OcppTransactionId.HasValue && 
                        request.TransactionId.HasValue && request.TransactionId.Value > 0)
                    {
                        session.OcppTransactionId = request.TransactionId.Value;
                        await context.SaveChangesAsync();
                        _logger.LogInformation("Updated session {SessionId} with OcppTransactionId {TransactionId} from MeterValues (StartTransaction was likely rejected)", 
                            session.Id, request.TransactionId.Value);
                    }
                    else if (session != null && !session.OcppTransactionId.HasValue && 
                             (!request.TransactionId.HasValue || request.TransactionId.Value == 0))
                    {
                        // MeterValues has TransactionId 0 or null - this means StartTransaction was rejected
                        // We can't update the session with a TransactionId, but we can still track energy
                        _logger.LogWarning("Session {SessionId} found but MeterValues has TransactionId 0. StartTransaction was likely rejected. Cannot update OcppTransactionId.", 
                            session.Id);
                    }
                }
            }
        }

        if (session != null && request.MeterValue.Any())
        {
            // Update current meter value (take first meter reading)
            var firstMeterValue = request.MeterValue.First();
            var energySample = firstMeterValue.SampledValue.FirstOrDefault(s => 
                s.Measurand == "Energy.Active.Import.Register" || 
                s.Measurand == "Energy.Active.Import.Interval" ||
                string.IsNullOrEmpty(s.Measurand));

            if (energySample != null && double.TryParse(energySample.Value, out var meterValue))
            {
                var energyConsumed = (decimal)(meterValue / 1000.0); // Convert Wh to kWh
                session.EnergyDelivered = energyConsumed;
                
                // Update cost based on current energy consumed
                session.Cost = await CalculateSessionCostAsync(context, session);
                
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Updated EnergyDelivered for session {SessionId} (TransactionId: {TransactionId}, ConnectorId: {ConnectorId}): {Energy}kWh", 
                    session.Id, request.TransactionId, request.ConnectorId, energyConsumed);
            }
            else
            {
                _logger.LogWarning("MeterValues received but no valid energy sample found. TransactionId: {TransactionId}, ConnectorId: {ConnectorId}, MeterValues: {MeterValues}", 
                    request.TransactionId, request.ConnectorId, 
                    string.Join(", ", firstMeterValue.SampledValue.Select(s => $"{s.Measurand}={s.Value}")));
            }
        }
        else
        {
            _logger.LogWarning("MeterValues received but no active session found. TransactionId: {TransactionId}, ConnectorId: {ConnectorId}, ChargeBoxId: {ChargeBoxId}", 
                request.TransactionId, request.ConnectorId, chargeBoxId);
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
            _logger.LogWarning("ServiceProviderFactory not available, cannot send notification for station {StationId}", stationId);
            return;
        }

        try
        {
            using var scope = _serviceProviderFactory().CreateScope();
            
            // Use reflection to get INotificationService to avoid hard dependency on Api project
            var notificationServiceType = Type.GetType("ChargingControlSystem.Api.Services.INotificationService, ChargingControlSystem.Api");
            if (notificationServiceType == null)
            {
                _logger.LogWarning("INotificationService type not found, cannot send notification for station {StationId}", stationId);
                return;
            }

            var notificationService = scope.ServiceProvider.GetService(notificationServiceType);
            if (notificationService == null)
            {
                _logger.LogWarning("INotificationService not found in service provider, cannot send notification for station {StationId}", stationId);
                return;
            }

            // Call NotifyStationStatusChangedAsync via reflection
            var method = notificationServiceType.GetMethod("NotifyStationStatusChangedAsync");
            if (method != null)
            {
                _logger.LogInformation("Sending SignalR notification: TenantId={TenantId}, StationId={StationId}, Status={Status}", 
                    tenantId, stationId, status);
                var task = method.Invoke(notificationService, new object[] { tenantId, stationId, status, message });
                if (task is Task notifyTask && notifyTask != null)
                {
                    await notifyTask;
                    _logger.LogInformation("Successfully sent SignalR notification for station {StationId}, Status={Status}", stationId, status);
                }
            }
            else
            {
                _logger.LogWarning("NotifyStationStatusChangedAsync method not found, cannot send notification for station {StationId}", stationId);
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
                if (task is Task notifyTask && notifyTask != null)
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

    /// <summary>
    /// Notify clients about session update (via SignalR)
    /// </summary>
    private async Task NotifySessionUpdateAsync(Guid tenantId, Guid userId, Guid sessionId, string status, string? message = null)
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
                _logger.LogWarning("INotificationService type not found, cannot send session update notification");
                return;
            }

            var notificationService = scope.ServiceProvider.GetService(notificationServiceType);
            if (notificationService == null)
            {
                _logger.LogWarning("INotificationService not found in service provider, cannot send session update notification");
                return;
            }

            // Call NotifySessionUpdateAsync via reflection
            var method = notificationServiceType.GetMethod("NotifySessionUpdateAsync");
            if (method != null)
            {
                _logger.LogInformation("Sending SignalR session update notification: TenantId={TenantId}, UserId={UserId}, SessionId={SessionId}, Status={Status}", 
                    tenantId, userId, sessionId, status);
                var task = method.Invoke(notificationService, new object[] { userId, sessionId, status, message });
                if (task is Task notifyTask && notifyTask != null)
                {
                    await notifyTask;
                    _logger.LogInformation("Successfully sent SignalR session update notification for session {SessionId}, Status={Status}", sessionId, status);
                }
            }
            else
            {
                _logger.LogWarning("NotifySessionUpdateAsync method not found, cannot send session update notification");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send session update notification for session {SessionId}", sessionId);
            // Don't throw - notification should not block the OCPP response
        }
    }
}
