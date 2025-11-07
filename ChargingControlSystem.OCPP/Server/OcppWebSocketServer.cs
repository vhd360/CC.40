using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ChargingControlSystem.OCPP.Models;
using ChargingControlSystem.OCPP.Handlers;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.OCPP.Server;

/// <summary>
/// OCPP WebSocket Server for handling charge point connections
/// </summary>
public class OcppWebSocketServer
{
    private readonly ILogger<OcppWebSocketServer> _logger;
    private readonly HttpListener _httpListener;
    private readonly ConcurrentDictionary<string, WebSocket> _connections;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cancellationTokenSource;

    public OcppWebSocketServer(
        IServiceProvider serviceProvider,
        ILogger<OcppWebSocketServer> logger,
        string prefix = "http://localhost:9000/ocpp/")
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(prefix);
        _connections = new ConcurrentDictionary<string, WebSocket>();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _httpListener.Start();
        _logger.LogInformation("OCPP WebSocket Server started on {Prefix}", string.Join(", ", _httpListener.Prefixes));

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var context = await _httpListener.GetContextAsync();
                _ = HandleConnectionAsync(context);
            }
            catch (Exception ex) when (ex is HttpListenerException || ex is ObjectDisposedException)
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error accepting connection");
                }
            }
        }
    }

    private async Task HandleConnectionAsync(HttpListenerContext context)
    {
        if (!context.Request.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            context.Response.Close();
            return;
        }

        // Extract ChargeBox ID from URL path
        var path = context.Request.Url?.AbsolutePath ?? "";
        var chargeBoxId = path.Split('/').LastOrDefault(s => !string.IsNullOrEmpty(s)) ?? "unknown";

        _logger.LogInformation("WebSocket connection request from ChargeBox: {ChargeBoxId}", chargeBoxId);

        WebSocketContext webSocketContext;
        try
        {
            webSocketContext = await context.AcceptWebSocketAsync(subProtocol: "ocpp1.6");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting WebSocket");
            context.Response.StatusCode = 500;
            context.Response.Close();
            return;
        }

        var webSocket = webSocketContext.WebSocket;
        _connections.TryAdd(chargeBoxId, webSocket);

        try
        {
            await HandleWebSocketAsync(chargeBoxId, webSocket);
        }
        finally
        {
            _connections.TryRemove(chargeBoxId, out _);
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            webSocket.Dispose();
            _logger.LogInformation("WebSocket connection closed for ChargeBox: {ChargeBoxId}", chargeBoxId);
            
            // Update station status to Unavailable when connection is lost
            await UpdateStationStatusOnDisconnectAsync(chargeBoxId);
        }
    }

    private async Task HandleWebSocketAsync(string chargeBoxId, WebSocket webSocket)
    {
        var buffer = new byte[4096];

        while (webSocket.State == WebSocketState.Open)
        {
            var messageBuilder = new StringBuilder();
            WebSocketReceiveResult result;

            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }
                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            var message = messageBuilder.ToString();
            _logger.LogDebug("Received from {ChargeBoxId}: {Message}", chargeBoxId, message);

            var response = await ProcessMessageAsync(chargeBoxId, message);
            if (!string.IsNullOrEmpty(response))
            {
                var responseBytes = Encoding.UTF8.GetBytes(response);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(responseBytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);

                _logger.LogDebug("Sent to {ChargeBoxId}: {Response}", chargeBoxId, response);
            }
        }
    }

    private async Task<string> ProcessMessageAsync(string chargeBoxId, string message)
    {
        try
        {
            // Configure JSON settings for OCPP (UTC timestamps)
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            // Parse OCPP message: [MessageTypeId, UniqueId, Action, Payload] or [MessageTypeId, UniqueId, Payload]
            var jArray = JArray.Parse(message);
            var messageTypeId = jArray[0].Value<int>();
            var uniqueId = jArray[1].Value<string>() ?? Guid.NewGuid().ToString();

            if (messageTypeId == (int)MessageType.CALL)
            {
                var action = jArray[2].Value<string>() ?? "";
                var payload = jArray[3];

                _logger.LogInformation("Processing {Action} from {ChargeBoxId}", action, chargeBoxId);

                // Create a scope and resolve the handler for this request
                using var scope = _serviceProvider.CreateScope();
                var messageHandler = scope.ServiceProvider.GetRequiredService<IOcppMessageHandler>();
                var responsePayload = await messageHandler.HandleMessageAsync(chargeBoxId, action, payload);

                // Build CALLRESULT: [3, UniqueId, Payload]
                var serializer = JsonSerializer.Create(jsonSettings);
                
                var response = new JArray(
                    (int)MessageType.CALLRESULT,
                    uniqueId,
                    JToken.FromObject(responsePayload, serializer)
                );

                return response.ToString(Formatting.None);
            }
            else if (messageTypeId == (int)MessageType.CALLRESULT)
            {
                // Handle responses to our requests (not implemented in this basic version)
                _logger.LogDebug("Received CALLRESULT for message {UniqueId}", uniqueId);
                return string.Empty;
            }
            else if (messageTypeId == (int)MessageType.CALLERROR)
            {
                var errorCode = jArray[2].Value<string>();
                var errorDescription = jArray[3].Value<string>();
                _logger.LogWarning("Received CALLERROR: {ErrorCode} - {ErrorDescription}", errorCode, errorDescription);
                return string.Empty;
            }
            else
            {
                return CreateErrorResponse(uniqueId, OcppErrorCode.ProtocolError, "Unknown message type");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error");
            return CreateErrorResponse(Guid.NewGuid().ToString(), OcppErrorCode.FormationViolation, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return CreateErrorResponse(Guid.NewGuid().ToString(), OcppErrorCode.InternalError, ex.Message);
        }
    }

    private string CreateErrorResponse(string uniqueId, string errorCode, string errorDescription)
    {
        var error = new JArray(
            (int)MessageType.CALLERROR,
            uniqueId,
            errorCode,
            errorDescription,
            new JObject()
        );
        return error.ToString(Formatting.None);
    }

    public async Task SendMessageAsync(string chargeBoxId, string action, object payload)
    {
        if (!_connections.TryGetValue(chargeBoxId, out var webSocket) || webSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException($"No active connection for ChargeBox {chargeBoxId}");
        }

        var uniqueId = Guid.NewGuid().ToString();
        
        // Use JsonSerializerSettings to ignore null values (OCPP spec requires this)
        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        var serializer = JsonSerializer.Create(jsonSettings);
        
        var message = new JArray(
            (int)MessageType.CALL,
            uniqueId,
            action,
            JToken.FromObject(payload, serializer)
        );

        var messageBytes = Encoding.UTF8.GetBytes(message.ToString(Formatting.None));
        await webSocket.SendAsync(
            new ArraySegment<byte>(messageBytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        _logger.LogInformation("Sent {Action} to {ChargeBoxId}", action, chargeBoxId);
    }

    private async Task UpdateStationStatusOnDisconnectAsync(string chargeBoxId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = await contextFactory.CreateDbContextAsync();

            var station = await context.ChargingStations
                .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);

            if (station != null)
            {
                station.Status = ChargingStationStatus.Unavailable;
                station.LastHeartbeat = null; // Clear last heartbeat when disconnected
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Set station {ChargeBoxId} status to Unavailable (WebSocket disconnected)", chargeBoxId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update station status for {ChargeBoxId} on disconnect", chargeBoxId);
        }
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource?.Cancel();
        _httpListener.Stop();
        
        var closeTasks = _connections.Values
            .Where(ws => ws.State == WebSocketState.Open)
            .Select(ws => ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutdown", CancellationToken.None));
        
        await Task.WhenAll(closeTasks);
        _logger.LogInformation("OCPP WebSocket Server stopped");
    }

    /// <summary>
    /// Check if a charging station is currently connected
    /// </summary>
    public bool IsStationConnected(string chargeBoxId)
    {
        return _connections.TryGetValue(chargeBoxId, out var webSocket) && 
               webSocket.State == WebSocketState.Open;
    }
}

