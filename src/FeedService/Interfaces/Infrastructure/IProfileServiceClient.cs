namespace FeedService.Interfaces.Infrastructure
{
    public interface IProfileServiceClient
    {
        Task<List<Guid>> GetFollowersAsync(Guid followingId);
        Task<List<Guid>> GetFollowingAsync(Guid followerId);
    }
}
