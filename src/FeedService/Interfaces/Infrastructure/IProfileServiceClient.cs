namespace FeedService.Interfaces.Infrastructure
{
    public interface IProfileServiceClient
    {
        Task<IEnumerable<Guid>> GetFollowersAsync(Guid followingId);
        Task<IEnumerable<Guid>> GetFollowingsAsync(Guid followerId, int count);
        Task<IEnumerable<Guid>> GetBlocked(Guid blockerId);
    }
}
