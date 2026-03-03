using AuthService.Models;

namespace AuthService.Interfaces.Infrastructure
{
    public interface IUserRepository
    {
        Task<User?> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UserExistsAsync(string email, string username);
        Task<bool> CanConnectAsync();
    }
}
