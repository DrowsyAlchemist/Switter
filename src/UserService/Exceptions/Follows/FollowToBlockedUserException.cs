namespace UserService.Exceptions.Follows
{
    public class FollowToBlockedUserException : FollowException
    {
        public FollowToBlockedUserException() : base("Can't follow to blocked user.")
        {
        }
    }
}
