using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ITweetRepository
    {
        Task<Tweet?> GetByIdAsync(Guid id);
        Task<List<Tweet>> GetByUserAsync(Guid userId);
        Task<bool> IsRetweetedAsync(Guid userId, Guid tweet);
        Task<List<Tweet>> GetRepliesAsync(Guid tweetId);

        Task<Tweet> AddAsync(Tweet tweet);
        Task<Tweet> UpdateAsync(Tweet tweet);
        Task<Tweet> DeleteAsync(Guid id);
    }
}
