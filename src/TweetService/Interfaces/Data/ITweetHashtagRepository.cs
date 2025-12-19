using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ITweetHashtagRepository
    {
        Task<List<TweetHashtag>> GetByHashtagAsync(string tag);
        int GetUsageCount(string tag, TimeSpan period);
        Task<TweetHashtag> AddAsync(TweetHashtag hashtag);
        Task<List<TweetHashtag>> AddRangeAsync(List<TweetHashtag> hashtags);
    }
}
