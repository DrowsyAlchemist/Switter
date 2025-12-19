using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITrendService
    {
        Task<List<string>> GetTrendCategoriesAsync();
        Task<List<TweetDto>> GetTrendTweetsAsync(Guid? userId);
        Task<List<TweetDto>> GetTrendTweetsAsync(string hashtag, Guid? userId);
    }
}
