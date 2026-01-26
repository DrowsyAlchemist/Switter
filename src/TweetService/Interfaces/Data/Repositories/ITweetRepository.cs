using TweetService.Models;

namespace TweetService.Interfaces.Data.Repositories
{
    public interface ITweetRepository
    {
        Task<Tweet?> GetByIdAsync(Guid id);
        Task<List<Tweet>> GetByIdsAsync(IEnumerable<Guid> ids, int page, int pageSize);
        Task<List<Tweet>> GetByHashtagAsync(IEnumerable<Guid> ids, string hashtag, int page, int pageSize);
        Task<List<Tweet>> GetByUserAsync(Guid userId, int page, int pageSize);
        Task<List<Guid>> GetIdsByUserAsync(Guid userId, int page, int pageSize);
        Task<bool> IsRetweetedAsync(Guid tweet, Guid userId);
        Task<List<Guid>> GetRetweetedIdsAsync(List<Guid> tweetIds, Guid userId);
        Task<List<Tweet>> GetRepliesAsync(Guid tweetId, int page, int pageSize);

        Task<Tweet> AddAsync(Tweet tweet);
        Task IncrementLikesCount(Guid tweetId);
        Task DecrementLikesCount(Guid tweetId);
        Task<Tweet> UpdateAsync(Tweet tweet);
        Task UpdateRangeAsync(List<Tweet> tweets);
        Task SoftDeleteAsync(Guid id);
        Task SoftDeleteRangeAsync(List<Guid> ids);
    }
}
