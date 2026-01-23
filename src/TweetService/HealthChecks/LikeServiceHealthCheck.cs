using Microsoft.Extensions.Diagnostics.HealthChecks;
using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.HealthChecks
{
    public class LikeServiceHealthCheck : IHealthCheck
    {
        private readonly static Guid TestGuid = Guid.Empty;
        private readonly ILikeService _likeService;
        private readonly ILikesRepository _likesRepository;
        private readonly ITweetRepository _tweetRepository;
        private readonly ILogger<LikeServiceHealthCheck> _logger;

        public LikeServiceHealthCheck(
            ILikeService likeService,
            ILikesRepository likesRepository,
            ITweetRepository tweetRepository,
            ILogger<LikeServiceHealthCheck> logger)
        {
            _likeService = likeService;
            _likesRepository = likesRepository;
            _tweetRepository = tweetRepository;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
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

                return isHealthy
                   ? HealthCheckResult.Healthy("Like service is working")
                   : HealthCheckResult.Unhealthy("Like service has problems"); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Like service health check failed");
                return HealthCheckResult.Unhealthy("Like service exception");
            }
        }
    }
}
