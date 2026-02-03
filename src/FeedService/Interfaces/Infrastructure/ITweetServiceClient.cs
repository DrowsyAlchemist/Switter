using FeedService.DTOs;

namespace FeedService.Interfaces.Infrastructure
{
    public interface ITweetServiceClient
    {
        Task<List<Guid>> GetRecentTweetsAsync(Guid userId, int count);
    }
}
