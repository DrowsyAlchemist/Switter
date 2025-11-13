namespace UserService.Exceptions.Follows
{
    public class SelfFollowException : FollowException
    {
        public SelfFollowException() : base("Cannot follow yourself")
        {
        }
    }
}