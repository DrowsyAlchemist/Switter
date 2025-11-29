using UserService.Models;

namespace UserService.Interfaces.Data
{
    public interface IProfilesRepository
    {
        Task<UserProfile> AddAsync(UserProfile profile);
        Task<List<UserProfile>> GetProfilesAsync();
        Task<UserProfile?> GetProfileByIdAsync(Guid userId);
        Task<UserProfile> UpdateProfileAsync(UserProfile profile);
        Task RemoveAsync(Guid id);
    }
}
