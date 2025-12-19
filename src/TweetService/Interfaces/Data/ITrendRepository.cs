using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ITrendRepository
    {
        Task<List<Tweet>> GetMostRetweetedAsync(int page = 1, int pageSize = 10);
        Task<List<Tweet>> GetMostRetweetedAsync(string hashtag, int page = 1, int pageSize = 10);
        Task<List<Tweet>> GetMostLikedAsync(int page = 1, int pageSize = 10);
        Task<List<Tweet>> GetMostLikedAsync(string hashtag, int page = 1, int pageSize = 10);
    }
}
