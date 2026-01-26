using TweetService.Models;

namespace TweetService.Interfaces.Data.Repositories
{
    public interface IHashtagRepository
    {
        Task<Hashtag?> GetByTagAsync(string tag);
        Task<List<Hashtag>> GetByTagsAsync(IEnumerable<string> tags);
        Task<List<Guid>> GetIdByTag(IEnumerable<string> tags);
        Task<List<Hashtag>> SearchAsync(string query, int page, int pageSize);
        Task<List<Hashtag>> GetMostPopularAsync(int count);
        Task<Hashtag> AddAsync(Hashtag hashtag);
        Task AddRangeAsync(List<string> hashtags);
        Task<Hashtag> IncrementUsageCounterAsync(string tag);
        Task IncrementUsageCounterAsync(List<string> tags);
        Task<bool> IsExist(string tag);
        Task<List<string>> GetExists(IEnumerable<string> tags);
    }
}
