namespace NotificationService.Models
{
    public class WebSocketConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }
}
