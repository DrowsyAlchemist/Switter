namespace UserService.KafkaEvents.UserEvents
{
    public record UserFollowedEvent(Guid FollowerId, Guid FolloweeId, DateTime Timestamp);
}
