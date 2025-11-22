namespace UserService.Exceptions.Follows
{
    public class FollowException : UserServiceException
    {
        public FollowException(string? message) : base(message) { }
    }
}