using Microsoft.AspNetCore.SignalR;
using NotificationService.DTOs;
using NotificationService.Hubs;
using NotificationService.Interfaces;

namespace NotificationService.Services
{
    public class WebSocketMessager : IWebSocketMessager
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IWebSocketConnectionRepository _connectionRepository;
        private readonly ILogger<WebSocketMessager> _logger;

        public WebSocketMessager(
            IHubContext<NotificationHub> hubContext,
            IWebSocketConnectionRepository connectionRepository,
            ILogger<WebSocketMessager> logger)
        {
            _hubContext = hubContext;
            _connectionRepository = connectionRepository;
            _logger = logger;
        }

        public async Task SendToUserAsync(Guid userId, WebSocketMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            var connections = _connectionRepository.GetUserConnections(userId);
            if (connections.Count == 0)
            {
                _logger.LogDebug("User {UserId} has no active WebSocket connections", userId);
                return;
            }

            foreach (var connectionId in connections)
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveNotification", message);
            }

            _logger.LogDebug("Sent WebSocket notification to {Count} connections of user {UserId}",
                connections.Count, userId);
        }

        public async Task BroadcastSystemMessageAsync(WebSocketMessage message)
        {
            await _hubContext.Clients.All.SendAsync("SystemNotification", message);
        }
    }
}
