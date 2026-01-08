using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ITweetRepository
    {
        Task<Tweet?> GetByIdAsync(Guid id);
        Task<List<Tweet>> GetByIdsAsync(List<Guid> ids);
        Task<List<Tweet>> GetByHashtagAsync(List<Guid> ids, string hashtag);
        Task<List<Tweet>> GetByUserAsync(Guid userId);
        Task<List<Guid>> GetIdsByUserAsync(Guid userId);
        Task<bool> IsRetweetedAsync(Guid tweet, Guid userId);
        Task<List<Guid>> GetRetweetedIdsAsync(List<Guid> tweetIds, Guid userId);
        Task<List<Tweet>> GetRepliesAsync(Guid tweetId);

        Task<Tweet> AddAsync(Tweet tweet);
        Task<Tweet> UpdateAsync(Tweet tweet);
        Task UpdateRangeAsync(List<Tweet> tweets);
        Task SoftDeleteAsync(Guid id);
        Task SoftDeleteRangeAsync(List<Guid> ids);
    }
}
