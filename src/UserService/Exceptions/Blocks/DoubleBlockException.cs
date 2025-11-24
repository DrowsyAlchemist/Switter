namespace UserService.Exceptions.Blocks
{
    public class DoubleBlockException : BlockException
    {
        public DoubleBlockException() : base("Block already exists.")
        {
        }
    }
}