using UserService.DTOs;

namespace UserService.Interfaces
{
    public interface IBlockService
    {
        Task BlockAsync(Guid blocker, Guid blocked);
        Task UnblockAsync(Guid blocker, Guid blocked);
        Task<bool> IsBlocked(Guid blocker, Guid blocked);
        Task<List<UserProfileDto>> GetBlockedAsync(Guid blockerId, int page = 1, int pageSize = 20);
    }
}
