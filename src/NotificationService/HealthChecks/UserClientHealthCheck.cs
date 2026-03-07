using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.Interfaces.Infrastructure;

namespace NotificationService.HealthChecks
{
    public class UserClientHealthCheck : IHealthCheck
    {
        private readonly IProfileServiceClient _profileServiceClient;

        public UserClientHealthCheck(IProfileServiceClient profileServiceClient)
        {
            _profileServiceClient = profileServiceClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = await _profileServiceClient.CheckConnectionAsync();
            return isHealthy
                   ? HealthCheckResult.Healthy("UserClient is working")
                   : HealthCheckResult.Unhealthy("UserClient connection failed");
        }
    }
}
