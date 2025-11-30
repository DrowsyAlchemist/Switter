namespace UserService.KafkaEvents.AuthEvents
{
    public record UserRegisteredEvent(Guid UserId, string Username, string Email, DateTime Timestamp);
}
