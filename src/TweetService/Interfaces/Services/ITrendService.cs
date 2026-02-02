using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITrendService
    {
        Task<IEnumerable<string>> GetTrendCategoriesAsync(int page, int pageSize);
        Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(int page, int pageSize);
        Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(string hashtag, int page, int pageSize);
    }
}
