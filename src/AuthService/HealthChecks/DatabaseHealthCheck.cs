using AuthService.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuthService.HealthChecks
{
    internal class DatabaseHealthCheck : IHealthCheck
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger _logger;

        public DatabaseHealthCheck(UserRepository userRepository , ILogger<DatabaseHealthCheck> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var isHealthy = await _userRepository.CanConnectAsync();
                return isHealthy
                    ? HealthCheckResult.Healthy("Database is working")
                    : HealthCheckResult.Unhealthy("Database connection failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy("Database exception");
            }
        }
    }
}
