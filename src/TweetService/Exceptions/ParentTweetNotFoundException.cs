namespace TweetService.Exceptions
{
    public class ParentTweetNotFoundException(Guid id) : Exception($"Parent tweet with id ({id}) not found.") { }
}
