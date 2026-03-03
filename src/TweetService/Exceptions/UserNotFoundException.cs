namespace TweetService.Exceptions
{
    public class UserNotFoundException(Guid id) : Exception($"User with ID:{id} not found.") { }
}
