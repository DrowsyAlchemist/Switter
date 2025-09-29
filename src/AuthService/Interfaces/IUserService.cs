using AuthService.Models;

namespace AuthService.Interfaces
{
    interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UserExistsAsync(string email, string username);
    }

}
