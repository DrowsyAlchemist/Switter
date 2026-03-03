using NotificationService.DTOs;

namespace NotificationService.Interfaces.Infrastructure
{
    public interface IProfileServiceClient
    {
        Task<UserInfo?> GetUserInfoAsync(Guid userId);
        Task<List<Guid>> GetFollowersIds(Guid userId);
    }
}
