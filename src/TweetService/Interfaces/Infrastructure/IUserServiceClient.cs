using TweetService.DTOs;

namespace TweetService.Interfaces.Infrastructure
{
    public interface IUserServiceClient
    {
        Task<UserInfo?> GetUserInfoAsync(Guid userId);
    }
}