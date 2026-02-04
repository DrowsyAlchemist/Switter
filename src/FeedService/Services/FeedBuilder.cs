using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;
using FeedService.Models.Options;

namespace FeedService.Services
{
    public class FeedBuilder : IFeedBuilder
    {
        private readonly IFeedFiller _filler;
        private readonly ITweetServiceClient _tweetServiceClient;
        private readonly IFollowsRepository _followsRepository;
        private readonly FeedOptions _options;

        public FeedBuilder(
            IFeedFiller filler,
            ITweetServiceClient tweetServiceClient,
            IFollowsRepository followsRepository,
            FeedOptions options)
        {
            _filler = filler;
            _tweetServiceClient = tweetServiceClient;
            _followsRepository = followsRepository;
            _options = options;
        }

        public async Task BuildFeedAsync(Guid userId)
        {
            await _filler.ClearFeedAsync(userId);

            var feedTweets = (await GetFollowingsTweetsAsync(userId))
                .Take(_options.AllFollowingsTweetsMaxCount)
                .ToList();

            var trendTweets = (await GetTrendTweetsAsync(userId))
                .Take(_options.MaxFeedSize - feedTweets.Count)
                .ToList();

            feedTweets.AddRange(trendTweets);
            await _filler.AddTweetsToFeedAsync(feedTweets, userId);
        }

        private async Task<List<Guid>> GetFollowingsTweetsAsync(Guid userId)
        {
            var followingIds = await _followsRepository.GetFollowingsAsync(userId, _options.FollowingsMaxCount);
            var tweetIds = new List<Guid>();

            foreach (var followingId in followingIds)
            {
                var recentTweets = await _tweetServiceClient.GetRecentUserTweetIdsAsync(followingId, _options.TweetsByEachFollowingMaxCount);
                tweetIds.AddRange(recentTweets);
            }
            return tweetIds;
        }

        private async Task<List<Guid>> GetTrendTweetsAsync(Guid userId)
        {
            var trendTweetIds = await _tweetServiceClient.GetTrendTweetsIdsAsync(_options.TrendTweetsMaxCount);

            var trendCategories = await _tweetServiceClient.GetTrendCategoriesAsync(_options.TrendCategoriesMaxCount);

            foreach (var category in trendCategories)
            {
                var tweetsInCategory = await _tweetServiceClient.GetTrendTweetsIdsAsync(category, _options.TrendTweetsInCategoryMaxCount);
                trendTweetIds.AddRange(tweetsInCategory);
            }
            return trendTweetIds;
        }
    }
}
