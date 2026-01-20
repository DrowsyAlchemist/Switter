using TweetService.DTOs;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;
using TweetService.Services.Infrastructure;

namespace TweetService.Services.Decorators
{
    public class LikeServiceWithUsage : ILikeService
    {
        private const string KeyForLikes = "KeyForTweetLikes";
        private readonly ILikeService _likeService;
        private readonly IRedisService _redisService;

        public LikeServiceWithUsage(ILikeService likeService, IRedisService redisService)
        {
            _likeService = likeService;
            _redisService = redisService;
        }

        public async Task<List<TweetDto>> GetLikedTweetsAsync(Guid userId, int page, int pageSize)
        {
            return await _likeService.GetLikedTweetsAsync(userId, page, pageSize);
        }

        public async Task LikeTweetAsync(Guid tweetId, Guid userId)
        {
            await _likeService.LikeTweetAsync(tweetId, userId);
            await _redisService.AddToListAsync(KeyForLikes, [tweetId.ToString()]);
        }

        public async Task UnlikeTweetAsync(Guid tweetId, Guid userId)
        {
            await _likeService.LikeTweetAsync(tweetId, userId);
        }
    }
}
