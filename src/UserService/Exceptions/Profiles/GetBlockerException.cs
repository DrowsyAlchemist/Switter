namespace UserService.Exceptions.Profiles
{
    public class GetBlockerException : UserServiceException
    {
        public GetBlockerException() : base() { }
        public GetBlockerException(string? message) : base(message) { }
    }
}