using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.Data;

namespace NotificationService.HealthChecks
{
    internal class DatabaseHealthCheck : IHealthCheck
    {
        private readonly NotificationDbContext _dbContext;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(NotificationDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var isHealthy = await _dbContext.CanConnectAsync();
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
