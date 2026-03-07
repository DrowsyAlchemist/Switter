using Microsoft.Extensions.Diagnostics.HealthChecks;
using TweetService.Interfaces.Infrastructure;

namespace TweetService.HealthChecks
{
    public class UserClientHealthCheck : IHealthCheck
    {
        private readonly IUserServiceClient _userServiceClient;

        public UserClientHealthCheck(IUserServiceClient userServiceClient)
        {
            _userServiceClient = userServiceClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = await _userServiceClient.CheckConnectionAsync();
            return isHealthy
                   ? HealthCheckResult.Healthy("UserClient is working")
                   : HealthCheckResult.Unhealthy("UserClient connection failed");
        }
    }
}
