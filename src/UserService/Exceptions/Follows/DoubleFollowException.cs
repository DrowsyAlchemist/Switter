namespace UserService.Exceptions.Follows
{
    public class DoubleFollowException : FollowExceprion
    {
        public DoubleFollowException() : base("Follow already exists.")
        {
        }
    }
}