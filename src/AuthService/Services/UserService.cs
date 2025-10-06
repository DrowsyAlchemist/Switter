using AuthService.Data;
using AuthService.Interfaces;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    internal class UserService : IUserService
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AuthDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError("Db is unavailable" + ex);
                throw new Exception("Db is unavailable" + ex);
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            ArgumentNullException.ThrowIfNull(email);

            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError("Db is unavailable" + ex);
                throw new Exception("Db is unavailable" + ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Db is unavailable" + ex);
                throw new Exception("Db is unavailable" + ex);
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            ArgumentNullException.ThrowIfNull(username);

            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError("Db is unavailable" + ex);
                throw new Exception("Db is unavailable" + ex);
            }
        }

        public async Task<bool> UserExistsAsync(string email, string username)
        {
            try
            {
                return await _context.Users
                    .AnyAsync(u => u.Email == email || u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError("Db is unavailable" + ex);
                throw new Exception("Db is unavailable" + ex);
            }
        }
    }
}
