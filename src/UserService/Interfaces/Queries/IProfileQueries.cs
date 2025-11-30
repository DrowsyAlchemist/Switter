using UserService.DTOs;

namespace UserService.Interfaces.Queries
{
    public interface IProfileQueries
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId, Guid? currentUserId = null);
        Task<List<UserProfileDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20);
    }
}
