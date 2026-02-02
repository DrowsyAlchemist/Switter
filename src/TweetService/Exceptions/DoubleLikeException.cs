namespace TweetService.Exceptions
{
    public class DoubleLikeException(Guid tweetId, Guid userId)
        : Exception($"User {userId} has already liked the tweet {tweetId}.")
    { }
}
