using UserService.DTOs;

namespace UserService.Interfaces.Queries
{
    public interface IBlockQueries
    {
        Task<bool> IsBlockedAsync(Guid blocker, Guid blocked);
        Task<List<UserProfileDto>> GetBlockedAsync(Guid blockerId, int page = 1, int pageSize = 20);
    }
}
