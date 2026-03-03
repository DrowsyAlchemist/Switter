using Microsoft.AspNetCore.SignalR;
using NotificationService.Interfaces;
using System.Security.Claims;

namespace NotificationService.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IWebSocketConnectionRepository _connectionManager;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(IWebSocketConnectionRepository connectionManager,
                             ILogger<NotificationHub> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("User not authenticated, closing connection");
                    Context.Abort();
                    return;
                }

                _connectionManager.AddConnection(
                    Context.ConnectionId,
                    userId.Value,
                    Context.GetHttpContext()?.Request.Headers.UserAgent,
                    Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString());

                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}",
                    userId.Value, Context.ConnectionId);

                await Clients.Client(Context.ConnectionId).SendAsync("Welcome", new
                {
                    message = "Connected to notification service",
                    userId = userId.Value,
                    timestamp = DateTime.UtcNow
                });

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                _connectionManager.RemoveConnection(Context.ConnectionId);

                _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);

                if (exception != null)
                    _logger.LogError(exception, "Disconnection error for {ConnectionId}", Context.ConnectionId);

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        public async Task SubscribeToTopic(string topic)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, topic);
            _logger.LogDebug("Connection {ConnectionId} subscribed to topic {Topic}",
                Context.ConnectionId, topic);
        }

        public async Task UnsubscribeFromTopic(string topic)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, topic);
            _logger.LogDebug("Connection {ConnectionId} unsubscribed from topic {Topic}",
                Context.ConnectionId, topic);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;
            return userId;
        }
    }
}