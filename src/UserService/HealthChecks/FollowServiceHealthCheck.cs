using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;
using UserService.Interfaces.Queries;
using UserService.Models;

namespace UserService.HealthChecks
{
    public class FollowServiceHealthCheck : IHealthCheck
    {
        private readonly ILogger<FollowServiceHealthCheck> _logger;
        private readonly IProfilesRepository _profilesRepository;
        private readonly IFollowCommands _commands;
        private readonly IFollowQueries _queries;

        public FollowServiceHealthCheck(ILogger<FollowServiceHealthCheck> logger,
            IProfilesRepository profilesRepository,
            IFollowCommands commands,
            IFollowQueries queries)
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
                var follower = new UserProfile()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "TestFollower"
                };
                var followee = new UserProfile()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "TestFollowee"
                };
                await _profilesRepository.AddAsync(follower);
                await _profilesRepository.AddAsync(followee);

                bool isHealthy = await TryFollow(follower.Id, followee.Id);

                isHealthy = isHealthy && await TryUnfollow(follower.Id, followee.Id);

                await _profilesRepository.RemoveAsync(follower.Id);
                await _profilesRepository.RemoveAsync(followee.Id);

                return isHealthy
                    ? HealthCheckResult.Healthy("Follow service is working")
                    : HealthCheckResult.Unhealthy("Follow service has problems"); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Follow service health check failed");
                return HealthCheckResult.Unhealthy("Follow service exception");
            }
        }

        private async Task<bool> TryFollow(Guid follower, Guid followee)
        {
            await _commands.FollowUserAsync(follower, followee);

            bool result = await _queries.IsFollowingAsync(follower, followee);

            var followers = await _queries.GetFollowersAsync(followee);
            result = result && followers.Any(u => u.Id == follower);

            var followings = await _queries.GetFollowingsAsync(follower);
            result = result && followings.Any(u => u.Id == followee);
            return result;
        }

        private async Task<bool> TryUnfollow(Guid follower, Guid followee)
        {
            await _commands.UnfollowUserAsync(follower, followee);

            bool result = await _queries.IsFollowingAsync(follower, followee) == false;

            var followers = await _queries.GetFollowersAsync(followee);
            result = result && followers.Any() == false;

            var followings = await _queries.GetFollowingsAsync(follower);
            result = result && followings.Any() == false;
            return result;
        }
    }
}
