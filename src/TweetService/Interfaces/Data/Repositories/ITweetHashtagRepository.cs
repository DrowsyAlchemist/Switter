using TweetService.Models;

namespace TweetService.Interfaces.Data.Repositories
{
    public interface ITweetHashtagRepository
    {
        Task<TweetHashtag> AddAsync(TweetHashtag hashtag);
        Task<List<TweetHashtag>> AddRangeAsync(List<TweetHashtag> hashtags);
    }
}
