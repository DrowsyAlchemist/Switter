using TweetService.DTOs;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators
{
    public class LikeServiceWithTrendFiller : ILikeService
    {
        private readonly TrendFiller _trendFiller;
        private readonly ILikeService _likeService;

        public LikeServiceWithTrendFiller(ILikeService likeService, TrendFiller trendFiller)
        {
            _trendFiller = trendFiller;
            _likeService = likeService;
        }

        public async Task<List<TweetDto>> GetLikedTweetsAsync(Guid userId, int page, int pageSize)
        {
            return await _likeService.GetLikedTweetsAsync(userId, page, pageSize);
        }

        public async Task LikeTweetAsync(Guid tweetId, Guid userId)
        {
            await _likeService.LikeTweetAsync(tweetId, userId);
            await _trendFiller.SetLikedTweetAsync(tweetId);
        }

        public async Task UnlikeTweetAsync(Guid tweetId, Guid userId)
        {
            await _likeService.LikeTweetAsync(tweetId, userId);
        }
    }
}
