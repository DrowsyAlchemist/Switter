namespace TweetService.Events
{
    public record class LikeCanceledEvent(Guid UserId, Guid TweetId, DateTime Timestamp);
}
