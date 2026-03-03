using NotificationService.Interfaces;
using NotificationService.Models;
using System.Collections.Concurrent;

namespace NotificationService.Services
{
    public class WebSocketConnectionInMemoryRepository : IWebSocketConnectionRepository
    {
        private readonly ConcurrentDictionary<string, WebSocketConnection> _connections = new();
        private readonly ConcurrentDictionary<Guid, List<string>> _userConnections = new();
        private readonly ILogger<WebSocketConnectionInMemoryRepository> _logger;

        public WebSocketConnectionInMemoryRepository(ILogger<WebSocketConnectionInMemoryRepository> logger)
        {
            _logger = logger;
        }

        public void AddConnection(string connectionId, Guid userId, string? userAgent, string? ipAddress)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentNullException(nameof(connectionId));

            var connection = new WebSocketConnection
            {
                ConnectionId = connectionId,
                UserId = userId,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                ConnectedAt = DateTime.UtcNow
            };

            _connections[connectionId] = connection;

            _userConnections.AddOrUpdate(userId,
                new List<string> { connectionId },
                (key, existingList) =>
                {
                    existingList.Add(connectionId);
                    return existingList;
                });

            _logger.LogDebug("Added connection {ConnectionId} for user {UserId}",
                connectionId, userId);
        }

        public void RemoveConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentNullException(nameof(connectionId));

            if (_connections.TryRemove(connectionId, out var connection))
            {
                if (_userConnections.TryGetValue(connection.UserId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                        _userConnections.TryRemove(connection.UserId, out _);
                }
                _logger.LogDebug("Removed connection {ConnectionId}", connectionId);
            }
        }

        public List<string> GetUserConnections(Guid userId)
        {
            return _userConnections.TryGetValue(userId, out var connections)
                ? [.. connections]
                : [];
        }

        public List<Guid> GetConnectedUsers() => [.. _userConnections.Keys];

        public int GetConnectionCount() => _connections.Count;

        public WebSocketConnection? GetConnection(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var connection)
                ? connection
                : null;
        }

        public void CleanupOldConnections(TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            var oldConnections = _connections.Values
                .Where(c => c.ConnectedAt < cutoff)
                .ToList();

            foreach (var connection in oldConnections)
                RemoveConnection(connection.ConnectionId);

            if (oldConnections.Count > 0)
                _logger.LogInformation("Cleaned up {Count} old connections", oldConnections.Count);
        }
    }
}
