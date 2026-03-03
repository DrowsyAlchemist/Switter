using NotificationService.DTOs;

namespace NotificationService.Interfaces
{
    public interface IWebSocketMessager
    {
        Task SendToUserAsync(Guid userId, WebSocketMessage message);
        Task BroadcastSystemMessageAsync(WebSocketMessage message);
    }
}
