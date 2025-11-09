namespace UserService.Exceptions.Follows
{
    public class FollowNotFoundException : FollowExceprion
    {
        public FollowNotFoundException(Guid follower, Guid followee):base($"Follow with follower: {follower}, followee: {followee} does not exist")
        {
        }
    }
}