namespace UserService.KafkaEvents.UserEvents
{
    public record UserUnfollowedEvent(Guid FollowerId, Guid FolloweeId, DateTime Timestamp);
}
