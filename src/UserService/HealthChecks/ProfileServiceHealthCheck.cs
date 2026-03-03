using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserService.DTOs;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;
using UserService.Interfaces.Queries;
using UserService.Models;

namespace UserService.HealthChecks
{
    public class ProfileServiceHealthCheck : IHealthCheck
    {
        private readonly ILogger<ProfileServiceHealthCheck> _logger;
        private readonly IProfilesRepository _repository;
        private readonly IProfileCommands _commands;
        private readonly IProfileQueries _queries;

        public ProfileServiceHealthCheck(IProfileCommands commands,
            IProfileQueries queries,
            IProfilesRepository repository,
            ILogger<ProfileServiceHealthCheck> logger)
        {
            _commands = commands;
            _queries = queries;
            _repository = repository;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {

                var testProfile = new UserProfile()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "TestUser",
                    Bio = "TestBio",
                    AvatarUrl = "TestAvatar"
                };
                bool isHealthy = await TryAddUser(testProfile);

                var updateRequest = new UpdateProfileRequest()
                {
                    DisplayName = "TestNewName",
                    AvatarUrl = "NewAvatar"
                };
                isHealthy = isHealthy && await TryUpdateUser(testProfile, updateRequest)
                                      && await TryRemoveUser(testProfile.Id);
                return isHealthy
                    ? HealthCheckResult.Healthy("Profile service is working")
                    : HealthCheckResult.Unhealthy("Profile service has problems"); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile service health check failed");
                return HealthCheckResult.Unhealthy("Profile service exception");
            }
        }

        private async Task<bool> TryAddUser(UserProfile testUser)
        {
            await _repository.AddAsync(testUser);
            var userFromService = await _queries.GetProfileAsync(testUser.Id);

            return userFromService != null
                && userFromService.DisplayName == testUser.DisplayName;
        }

        private async Task<bool> TryUpdateUser(UserProfile testUser, UpdateProfileRequest updateRequest)
        {
            await _commands.UpdateProfileAsync(testUser.Id, updateRequest);
            var updatedUser = await _queries.GetProfileAsync(testUser.Id);

            return updatedUser != null
                && updatedUser.DisplayName == updateRequest.DisplayName
                && updatedUser.Bio == testUser.Bio
                && updatedUser.AvatarUrl == updateRequest.AvatarUrl;
        }

        private async Task<bool> TryRemoveUser(Guid id)
        {
            await _repository.RemoveAsync(id);
            var removedUser = await _repository.GetProfileByIdAsync(id);
            return removedUser == null;
        }
    }
}
