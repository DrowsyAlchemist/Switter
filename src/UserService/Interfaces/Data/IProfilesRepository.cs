using UserService.Models;

namespace UserService.Interfaces.Data
{
    public interface IProfilesRepository
    {
        Task<List<UserProfile>> GetUsersAsync();
        Task<UserProfile> GetProfileAsync(Guid userId);
        Task<UserProfile> UpdateProfileAsync(UserProfile profile);
    }
}
