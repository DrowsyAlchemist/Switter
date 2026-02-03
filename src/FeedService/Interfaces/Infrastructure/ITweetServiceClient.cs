using FeedService.DTOs;

namespace FeedService.Interfaces.Infrastructure
{
    public interface ITweetServiceClient
    {
        Task<IEnumerable<TweetDto>> GetTweetsByIds(IEnumerable<Guid> ids);
        Task<List<Guid>> GetRecentTweetsAsync(Guid userId, int count);
    }
}
