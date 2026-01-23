using TweetService.DTOs;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators
{
    public class TrendServiceWithUserRelationship : ITrendService
    {
        private readonly ITrendService _trendService;
        private readonly IUserTweetRelationship _userTweetRelationship;

        public TrendServiceWithUserRelationship(ITrendService trendService, IUserTweetRelationship userTweetRelationship)
        {
            _trendService = trendService;
            _userTweetRelationship = userTweetRelationship;
        }

        public async Task<IEnumerable<string>> GetTrendCategoriesAsync(int page, int pageSize)
        {
            return await _trendService.GetTrendCategoriesAsync(page, pageSize);
        }

        public async Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(Guid? userId, int page, int pageSize)
        {
            var trendTweetDtos = await _trendService.GetTrendTweetsAsync(userId, page, pageSize);

            if (userId.HasValue)
                trendTweetDtos = await _userTweetRelationship.GetTweetsWithRelationshipsAsync(trendTweetDtos, userId.Value);

            return trendTweetDtos;
        }

        public async Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(string hashtag, Guid? userId, int page, int pageSize)
        {
            var trendTweetDtos = await _trendService.GetTrendTweetsAsync(hashtag, userId, page, pageSize);

            if (userId.HasValue)
                trendTweetDtos = await _userTweetRelationship.GetTweetsWithRelationshipsAsync(trendTweetDtos, userId.Value);

            return trendTweetDtos;
        }
    }
}
