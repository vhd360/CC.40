using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChargingControlSystem.Api.Hubs;

// TEMPORÄR: [Authorize] deaktiviert für Debugging
// [Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var tenantId = Context.User?.FindFirst("TenantId")?.Value;

        _logger.LogInformation("SignalR connection attempt - User authenticated: {IsAuthenticated}, UserId: {UserId}, TenantId: {TenantId}", 
            Context.User?.Identity?.IsAuthenticated, userId ?? "null", tenantId ?? "null");

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(tenantId))
        {
            // Add user to their tenant group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            _logger.LogInformation("User {UserId} connected to notification hub (Tenant: {TenantId})", userId, tenantId);
        }
        else if (Context.User?.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Authenticated user connected but UserId or TenantId is missing");
        }
        else
        {
            _logger.LogWarning("Unauthenticated connection attempt to SignalR hub");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
        await base.OnDisconnectedAsync(exception);
    }
}

