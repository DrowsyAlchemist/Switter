namespace UserService.Exceptions.Follows
{
    public class DoubleFollowException : FollowException
    {
        public DoubleFollowException() : base("Follow already exists.")
        {
        }
    }
}