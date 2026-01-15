using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITrendService
    {
        Task<List<string>> GetTrendCategoriesAsync();
        Task<List<TweetDto>> GetTrendTweetsAsync(Guid? userId, int page, int pageSize);
        Task<List<TweetDto>> GetTrendTweetsAsync(string hashtag, Guid? userId, int page, int pageSize);
    }
}
