using Microsoft.Extensions.Diagnostics.HealthChecks;
using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.HealthChecks
{
    public class TrendServiceHealthCheck : IHealthCheck
    {
        private readonly static Guid TestGuid1 = Guid.Empty;
        private readonly string _testHashtag = "test";

        private readonly ITweetCommands _tweetCommands;
        private readonly ITrendService _trendService;
        private readonly ILikeService _likeService;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<TweetServiceHealthCheck> _logger;

        public TrendServiceHealthCheck(
            ITweetCommands tweetCommands,
            ITrendService trendService,
            ILikeService likeService,
            ITransactionManager transactionManager,
            ILogger<TweetServiceHealthCheck> logger)
        {
            _tweetCommands = tweetCommands;
            _trendService = trendService;
            _likeService = likeService;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
            try
            {
                var testUserInfo = new UserInfo
                {
                    Id = TestGuid1,
                    DisplayName = "SwitterTrendsUser"
                };
                var tweetRequest = new CreateTweetRequest
                {
                    Type = TweetType.Tweet,
                    Content = $"Hello switter! #{_testHashtag}"
                };
                var tweet = await _tweetCommands.TweetAsync(testUserInfo, tweetRequest);

                // TrendCategories
                var trendCategories = await _trendService.GetTrendCategoriesAsync(1, 10);
                var categoriesCount = trendCategories.Count();
                bool isHealthy = categoriesCount > 0;
                if (categoriesCount < 10)
                    isHealthy = trendCategories.Contains(_testHashtag);

                // TrendTweets
                await _likeService.LikeTweetAsync(tweet.Id, testUserInfo.Id);
                var trendTweets = await _trendService.GetTrendTweetsAsync(1, 10);
                var trendTweetsCount = trendTweets.Count();
                isHealthy = trendTweetsCount > 0;
                if (trendTweetsCount < 10)
                    isHealthy = trendTweets.Any(t => t.Id == tweet.Id);

                await _tweetCommands.DeleteTweetAsync(tweet.Id, testUserInfo.Id);

                await transaction.CommitAsync(cancellationToken);

                return isHealthy
                   ? HealthCheckResult.Healthy("Trend service is working")
                   : HealthCheckResult.Unhealthy("Trend service has problems"); ;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Trend service health check failed");
                return HealthCheckResult.Unhealthy("Trend service exception");
            }
        }
    }
}
