using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITrendService
    {
        Task<List<string>> GetTrendCategoriesAsync(int page, int pageSize);
        Task<List<TweetDto>> GetTrendTweetsAsync(int page, int pageSize);
        Task<List<TweetDto>> GetTrendTweetsAsync(string hashtag, int page, int pageSize);
        Task<List<Guid>> GetTrendTweetIdsAsync(int page, int pageSize);
        Task<List<Guid>> GetTrendTweetIdsAsync(string hashtag, int page, int pageSize);
    }
}
