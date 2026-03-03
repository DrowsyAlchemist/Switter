namespace TweetService.Events
{
    public record class LikeSetEvent(Guid UserId, Guid TweetId, DateTime Timestamp);
}
