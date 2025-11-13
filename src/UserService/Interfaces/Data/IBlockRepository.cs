using UserService.Models;

namespace UserService.Interfaces.Data
{
    public interface IBlockRepository
    {
        Task<Block> AddAsync(Guid blockerId, Guid blockedId);
        Task<Block?> GetAsync(Guid blockerId, Guid blockedId);
        Task DeleteAsync(Guid blockerId, Guid blockedId);
        Task<List<UserProfile>> GetBlockedAsync(Guid blockerId);
        Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId);
    }
}
