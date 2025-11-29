using Microsoft.EntityFrameworkCore;
using UserService.Interfaces.Data;
using UserService.Models;

namespace UserService.Data
{
    public class ProfilesRepository : IProfilesRepository
    {
        private readonly UserDbContext _context;
        private readonly ILogger<ProfilesRepository> _logger;

        public ProfilesRepository(UserDbContext context, ILogger<ProfilesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<UserProfile>> GetProfilesAsync()
        {
            try
            {
                return await _context.Profiles.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<UserProfile?> GetProfileByIdAsync(Guid userId)
        {
            try
            {
                return await _context.Profiles
                    .Where(p => p.Id == userId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<UserProfile> AddAsync(UserProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            try
            {
                bool isExist = await _context.Profiles.AnyAsync(p => p.Id.Equals(profile.Id));
                if (isExist)
                    throw new InvalidOperationException("Profile with this id already exists.");

                await _context.Profiles.AddAsync(profile);
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<UserProfile> UpdateProfileAsync(UserProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            try
            {
                _context.Profiles.Update(profile);
                await _context.SaveChangesAsync();
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }
    }
}
