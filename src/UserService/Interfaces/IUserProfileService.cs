using UserService.DTOs;

namespace UserService.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetProfileAsync(Guid userId, Guid? currentUserId = null);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<List<UserProfileDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20);
    }
}
