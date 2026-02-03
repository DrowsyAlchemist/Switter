namespace FeedService.Interfaces.Infrastructure
{
    public interface IProfileServiceClient
    {
        Task<List<Guid>> GetFollowersAsync(Guid followingId);
    }
}
