namespace TweetService.Exceptions
{
    public class SelfRetweetException(Guid tweetId, Guid userId) :
        Exception($"User can't retweet their own tweet.\nTweet: {tweetId}\nUser: {userId}")
    { }
}
