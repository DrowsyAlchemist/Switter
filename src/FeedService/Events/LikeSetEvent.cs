namespace FeedService.Events
{
    public record class LikeSetEvent(Guid UserId, Guid TweetId, DateTime Timestamp);
}
