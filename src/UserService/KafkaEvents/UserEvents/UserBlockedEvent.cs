namespace UserService.KafkaEvents.UserEvents
{
    public record UserBlockedEvent(Guid BlockerId, Guid BlockedId, DateTime Timestamp);
}
