using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;
using UserService.Interfaces.Queries;
using UserService.Models;

namespace UserService.HealthChecks
{
    public class BlockServiceHealthCheck : IHealthCheck
    {
        private readonly ILogger<BlockServiceHealthCheck> _logger;
        private readonly IProfilesRepository _profilesRepository;
        private readonly IBlockCommands _commands;
        private readonly IBlockQueries _queries;

        public BlockServiceHealthCheck(ILogger<BlockServiceHealthCheck> logger,
            IProfilesRepository profilesRepository,
            IBlockCommands commands,
            IBlockQueries queries)
        {
            _logger = logger;
            _profilesRepository = profilesRepository;
            _commands = commands;
            _queries = queries;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var blocker = new UserProfile()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "TestBlocker"
                };
                var blocked = new UserProfile()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "TestBlocked"
                };
                await _profilesRepository.AddAsync(blocker);
                await _profilesRepository.AddAsync(blocked);

                bool isHealthy = await TryBlock(blocker.Id, blocked.Id);

                isHealthy = isHealthy && await TryUnblock(blocker.Id, blocked.Id);

                await _profilesRepository.RemoveAsync(blocker.Id);
                await _profilesRepository.RemoveAsync(blocked.Id);

                return isHealthy
                    ? HealthCheckResult.Healthy("Block service is working")
                    : HealthCheckResult.Unhealthy("Block service has problems"); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Block service health check failed");
                return HealthCheckResult.Unhealthy("Block service exception");
            }
        }

        private async Task<bool> TryBlock(Guid blocker, Guid blocked)
        {
            await _commands.BlockAsync(blocker, blocked);

            bool result = await _queries.IsBlockedAsync(blocker, blocked);

            var blockedUsers = await _queries.GetBlockedAsync(blocker, 1, int.MaxValue);
            result = result && blockedUsers.Any(u => u.Id == blocked);

            return result;
        }

        private async Task<bool> TryUnblock(Guid blocker, Guid blocked)
        {
            await _commands.UnblockAsync(blocker, blocked);

            bool result = await _queries.IsBlockedAsync(blocker, blocked) == false;

            var blockedUsers = await _queries.GetBlockedAsync(blocker, 1, int.MaxValue);
            result = result && blockedUsers.Any();
            return result;
        }
    }
}
