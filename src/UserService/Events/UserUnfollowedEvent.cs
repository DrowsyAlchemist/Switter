namespace UserService.Events
{
    public class UserUnfollowedEvent : KafkaEvent
    {
        public Guid FollowerId { get; set; }
        public Guid FolloweeId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
