using UserService.Models;

namespace UserService.Interfaces.Data
{
    public interface IBlockRepository
    {
        Task<Block> AddAsync(Guid blockerId, Guid blockedId);
        Task DeleteAsync(Guid blockerId, Guid blockedId);

        Task<Block?> GetAsync(Guid blockerId, Guid blockedId);
        Task<IEnumerable<UserProfile>> GetBlockedAsync(Guid blockerId, int page, int pageSize);
        Task<IEnumerable<Guid>> GetBlockedIdsAsync(Guid blockerId, int page, int pageSize);
        Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId);
    }
}