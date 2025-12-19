namespace TweetService.Exceptions
{
    public class LikeNotFoundException(Guid tweetId, Guid userId)
        : Exception($"Like not found.\nUser: {userId}\nTweet: {tweetId}")
    {
    }
}
