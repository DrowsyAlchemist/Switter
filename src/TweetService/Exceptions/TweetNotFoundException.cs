namespace TweetService.Exceptions
{
    public class TweetNotFoundException(Guid id) : Exception($"Tweet with id {id} not found.") { }
}
