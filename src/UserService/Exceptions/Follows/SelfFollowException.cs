namespace UserService.Exceptions.Follows
{
    public class SelfFollowException : FollowExceprion
    {
        public SelfFollowException() : base("Cannot follow yourself")
        {
        }
    }
}