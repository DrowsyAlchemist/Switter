using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITweetQueries
    {
        Task<TweetDto> GetTweetAsync(Guid tweetId);
        Task<IEnumerable<TweetDto>> GetTweetRepliesAsync(Guid tweetId, int page = 1, int pageSize = 20);
        Task<IEnumerable<TweetDto>> GetUserTweetsAsync(Guid userId, int page = 1, int pageSize = 20);
    }
}
