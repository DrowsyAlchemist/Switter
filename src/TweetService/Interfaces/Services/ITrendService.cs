using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITrendService
    {
        Task<IEnumerable<string>> GetTrendCategoriesAsync(int page, int pageSize);
        Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(Guid? userId, int page, int pageSize);
        Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(string hashtag, Guid? userId, int page, int pageSize);
    }
}
