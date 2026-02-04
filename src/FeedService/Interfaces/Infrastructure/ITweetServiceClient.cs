using FeedService.DTOs;

namespace FeedService.Interfaces.Infrastructure
{
    public interface ITweetServiceClient
    {
        Task<IEnumerable<TweetDto>> GetTweetsByIdAsync(IEnumerable<Guid> tweetIds);
        Task<List<Guid>> GetRecentUserTweetIdsAsync(Guid userId, int count);//////////////////////////////

        Task<List<string>> GetTrendCategoriesAsync(int count);
        Task<List<Guid>> GetTrendTweetsIdsAsync(int count);
        Task<List<Guid>> GetTrendTweetsIdsAsync(string hashtag, int count);
    }
}