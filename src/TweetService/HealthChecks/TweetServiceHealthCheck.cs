using Microsoft.Extensions.Diagnostics.HealthChecks;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.HealthChecks
{
    public class TweetServiceHealthCheck : IHealthCheck
    {
        private readonly static Guid TestGuid1 = Guid.Empty;
        private readonly static Guid TestGuid2 = Guid.Parse("22345200-abe8-4f60-90c8-0d43c5f6c0f6");
        private readonly ITweetCommands _tweetCommands;
        private readonly ITweetQueries _tweetQueries;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<TweetServiceHealthCheck> _logger;

        public TweetServiceHealthCheck(
            ITweetCommands tweetCommands,
            ITweetQueries tweetQueries,
            ITransactionManager transactionManager,
            ILogger<TweetServiceHealthCheck> logger)
        {
            _tweetCommands = tweetCommands;
            _tweetQueries = tweetQueries;
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
                    DisplayName = "SwitterTweetsUser"
                };
                var tweetRequest = new CreateTweetRequest
                {
                    Type = TweetType.Tweet,
                    Content = "Hello switter!"
                };
                var tweet = await _tweetCommands.TweetAsync(testUserInfo, tweetRequest);
                var tweetInDb = await _tweetQueries.GetTweetAsync(tweet.Id);

                bool isHealthy =
                    tweetInDb != null
                    && tweetInDb.AuthorId == testUserInfo.Id
                    && tweetInDb.AuthorDisplayName.Equals(testUserInfo.DisplayName)
                    && tweetInDb.Type == tweetRequest.Type
                    && tweetInDb.Content.Equals(tweetRequest.Content)
                    && tweetInDb.IsDeleted == false;

                var anotherTestUserInfo = new UserInfo
                {
                    Id = TestGuid2,
                    DisplayName = "AnotherSwitterTweetsUser"
                };
                var retweetRequest = new CreateTweetRequest
                {
                    Type = TweetType.Retweet,
                    ParentTweetId = tweet.Id,
                    Content = "Nice tweet!"
                };
                var replyRequest = new CreateTweetRequest
                {
                    Type = TweetType.Reply,
                    ParentTweetId = tweet.Id,
                    Content = "Nice tweet!"
                };
                var retweet = await _tweetCommands.TweetAsync(anotherTestUserInfo, retweetRequest);
                var reply = await _tweetCommands.TweetAsync(anotherTestUserInfo, replyRequest);

                tweetInDb = await _tweetQueries.GetTweetAsync(tweet.Id);

                isHealthy = isHealthy
                    && tweetInDb.RepliesCount == 1
                    && tweetInDb.RetweetsCount == 1;
                await _tweetCommands.DeleteTweetAsync(retweet.Id, anotherTestUserInfo.Id);
                await _tweetCommands.DeleteTweetAsync(reply.Id, anotherTestUserInfo.Id);
                await _tweetCommands.DeleteTweetAsync(tweet.Id, testUserInfo.Id);

                try
                {
                    tweetInDb = await _tweetQueries.GetTweetAsync(tweet.Id);
                    isHealthy = false;
                }
                catch (TweetNotFoundException) { }
                try
                {
                    var replyInDb = await _tweetQueries.GetTweetAsync(reply.Id);
                    isHealthy = false;
                }
                catch (TweetNotFoundException) { }
                try
                {
                    var retweetInDb = await _tweetQueries.GetTweetAsync(retweet.Id);
                    isHealthy = false;
                }
                catch (TweetNotFoundException) { }

                await transaction.CommitAsync(cancellationToken);

                return isHealthy
                   ? HealthCheckResult.Healthy("Tweet service is working")
                   : HealthCheckResult.Unhealthy("Tweet service has problems"); ;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Tweet service health check failed");
                return HealthCheckResult.Unhealthy("Tweet service exception");
            }
        }
    }
}
