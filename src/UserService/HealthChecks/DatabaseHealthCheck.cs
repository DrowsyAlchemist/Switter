using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserService.Data;

namespace UserService.HealthChecks
{
    internal class DatabaseHealthCheck : IHealthCheck
    {
        private readonly UserDbContext _dbContext;
        private readonly ILogger _logger;

        public DatabaseHealthCheck(UserDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
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
