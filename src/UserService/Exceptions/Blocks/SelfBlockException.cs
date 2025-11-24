namespace UserService.Exceptions.Blocks
{
    public class SelfBlockException : BlockException
    {
        public SelfBlockException() : base("Cannot block yourself")
        {
        }
    }
}