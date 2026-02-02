using Microsoft.Extensions.Diagnostics.HealthChecks;
using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Data.Repositories;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.HealthChecks
{
    public class LikeServiceHealthCheck : IHealthCheck
    {
        private readonly static Guid TestGuid = Guid.Empty;
        private readonly ILikeService _likeService;
        private readonly ITransactionManager _transactionManager;
        private readonly ITweetRepository _tweetRepository;
        private readonly ILogger<LikeServiceHealthCheck> _logger;

        public LikeServiceHealthCheck(
            ILikeService likeService,
            ITransactionManager transactionManager,
            ITweetRepository tweetRepository,
            ILogger<LikeServiceHealthCheck> logger)
        {
            _likeService = likeService;
            _transactionManager = transactionManager;
            _tweetRepository = tweetRepository;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
            try
            {
                var testUserInfo = new UserInfo
                {
                    Id = TestGuid,
                    DisplayName = "SwitterUser"
                };
                var testTweet = new Tweet
                {
                    Type = TweetType.Tweet,
                    Content = "Hello switter!",
                    AuthorId = testUserInfo.Id,
                    AuthorDisplayName = testUserInfo.DisplayName,
                };
                var tweet = await _tweetRepository.AddAsync(testTweet);
                await _likeService.LikeTweetAsync(tweet.Id, testUserInfo.Id);
                var likedTweets = await _likeService.GetLikedTweetsAsync(testUserInfo.Id, 1, 100);
                tweet = await _tweetRepository.GetByIdAsync(tweet.Id);

                bool isHealthy =
                    tweet != null
                    && tweet.LikesCount == 1
                    && likedTweets != null
                    && likedTweets.Any(t => t.Id == tweet.Id);

                await _likeService.UnlikeTweetAsync(tweet.Id, testUserInfo.Id);
                likedTweets = await _likeService.GetLikedTweetsAsync(testUserInfo.Id, 1, 100);
                tweet = await _tweetRepository.GetByIdAsync(tweet.Id);

                isHealthy = isHealthy
                    && tweet != null
                    && tweet.LikesCount == 0
                    && likedTweets != null
                    && likedTweets.Any(t => t.Id == tweet.Id) == false;

                await _tweetRepository.SoftDeleteAsync(tweet!.Id);

                await transaction.CommitAsync(cancellationToken);

                return isHealthy
                   ? HealthCheckResult.Healthy("Like service is working")
                   : HealthCheckResult.Unhealthy("Like service has problems");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Like service health check failed");
                return HealthCheckResult.Unhealthy("Like service exception");
            }
        }
    }
}
