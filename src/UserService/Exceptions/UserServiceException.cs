namespace UserService.Exceptions
{
    public class UserServiceException : Exception
    {
        public UserServiceException(string? message) : base(message)
        {
        }
    }
}