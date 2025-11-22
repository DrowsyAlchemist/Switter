namespace UserService.Exceptions
{
    public class UserServiceException : Exception
    {
        public UserServiceException() : base() { }
        public UserServiceException(string? message) : base() { }
        public UserServiceException(string? message, Exception? inner) : base(message, inner) { }
    }
}