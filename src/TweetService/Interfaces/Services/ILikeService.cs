using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ILikeService
    {
        Task LikeTweetAsync(Guid tweetId, Guid userId);
        Task UnlikeTweetAsync(Guid tweetId, Guid userId);
        Task<List<TweetDto>> GetLikedTweetsAsync(Guid userId, int page = 1, int pageSize = 20);
    }
}
