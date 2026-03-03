namespace TweetService.Exceptions
{
    public class DeleteTweetForbiddenException(Guid tweetId, Guid userID)
        : Exception($"User {userID} is not an author of tweet {tweetId} tweet.")
    { }
}
