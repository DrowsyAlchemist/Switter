namespace UserService.Exceptions.Blocks
{
    public class BlockException : UserServiceException
    {
        public BlockException(string? message) : base(message)
        {
        }
    }
}