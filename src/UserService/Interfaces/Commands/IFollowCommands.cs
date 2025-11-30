namespace UserService.Interfaces.Commands
{
    public interface IFollowCommands
    {
        Task FollowUserAsync(Guid followerId, Guid followeeId);
        Task UnfollowUserAsync(Guid followerId, Guid followeeId);
    }
}
