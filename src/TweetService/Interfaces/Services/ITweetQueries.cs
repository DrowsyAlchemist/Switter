using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITweetQueries
    {
        Task<TweetDto> GetTweetAsync(Guid tweetId);
        Task<List<TweetDto>> GetTweetsAsync(List<Guid> tweetIds);
        Task<List<TweetDto>> GetTweetRepliesAsync(Guid tweetId, int page, int pageSize);
        Task<List<TweetDto>> GetUserTweetsAsync(Guid userId, int page, int pageSize);
        Task<List<Guid>> GetUserTweetIdsAsync(Guid userId, int page, int pageSize);
    }
}