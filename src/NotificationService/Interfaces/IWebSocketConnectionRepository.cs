using NotificationService.Models;

namespace NotificationService.Interfaces
{
    public interface IWebSocketConnectionRepository
    {
        void AddConnection(string connectionId, Guid userId, string? userAgent, string? ipAddress);
        void RemoveConnection(string connectionId);

        WebSocketConnection? GetConnection(string connectionId);
        List<string> GetUserConnections(Guid userId);
        List<Guid> GetConnectedUsers();
        int GetConnectionCount();

        void CleanupOldConnections(TimeSpan maxAge);
    }
}
