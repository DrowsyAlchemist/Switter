using UserService.DTOs;

namespace UserService.Interfaces
{
    public interface IFollowersCounter
    {
        Task IncrementCounter(Guid followerId, Guid followeeId);
        Task DecrementCounter(Guid followerId, Guid followeeId);
        Task<UserProfileDto> ForceUpdateCountersForUserAsync(Guid userId);
    }
}
