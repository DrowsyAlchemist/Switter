namespace UserService.Exceptions.Blocks
{
    public class BlockNotFoundException : BlockException
    {
        public BlockNotFoundException(Guid blocker, Guid blocked) : base($"User {blocker} have not blocked user {blocked}.")
        {
        }
    }
}