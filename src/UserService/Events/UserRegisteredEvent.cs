namespace UserService.Events
{
    public class UserRegisteredEvent : KafkaEvent
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
