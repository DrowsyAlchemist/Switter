using FeedService.Interfaces.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FeedService.HealthChecks
{
    public class TweetClientHealthCheck : IHealthCheck
    {
        private readonly ITweetServiceClient _tweetServiceClient;

        public TweetClientHealthCheck(ITweetServiceClient tweetServiceClient)
        {
            _tweetServiceClient = tweetServiceClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = await _tweetServiceClient.CheckConnectionAsync();
            return isHealthy
                   ? HealthCheckResult.Healthy("TweetClient is working")
                   : HealthCheckResult.Unhealthy("TweetClient connection failed");
        }
    }
}
