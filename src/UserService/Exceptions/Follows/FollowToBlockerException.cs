namespace UserService.Exceptions.Follows
{
    public class FollowToBlockerException : FollowException
    {
        public FollowToBlockerException() : base("Follow is forbidden by other user.")
        {
        }
    }
}
