using UserService.DTOs;

namespace UserService.Interfaces.Queries
{
    public interface IBlockQueries
    {
        Task<bool> IsBlockedAsync(Guid blocker, Guid blocked);
        Task<IEnumerable<UserProfileDto>> GetBlockedAsync(Guid blockerId, int page, int pageSize);
        Task<IEnumerable<Guid>> GetBlockedIdsAsync(Guid blockerId, int page, int pageSize);
    }
}
