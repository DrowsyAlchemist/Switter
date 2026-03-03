namespace NotificationService.DTOs
{
    public class WebSocketMessage
    {
        public string Type { get; set; } = string.Empty; // "notification", "ping", "system"
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
