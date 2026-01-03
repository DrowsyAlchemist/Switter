namespace TweetService.Exceptions
{
    public class ParentTweetNullException() : Exception("Parent tweet for retweet or reply is null.") { }
}
