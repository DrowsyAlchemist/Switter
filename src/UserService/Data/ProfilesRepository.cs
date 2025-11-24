using Microsoft.EntityFrameworkCore;
using UserService.Interfaces.Data;
using UserService.Models;

namespace UserService.Data
{
    public class ProfilesRepository : IProfilesRepository
    {
        private readonly UserDbContext _context;

        public ProfilesRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserProfile>> GetUsersAsync()
        {
            return await _context.Profiles.AsNoTracking().ToListAsync();
        }

        public async Task<UserProfile?> GetProfileAsync(Guid userId)
        {
            return await _context.Profiles
                .Where(p => p.Id == userId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<UserProfile> AddAsync(UserProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            bool isExist = await _context.Profiles.AnyAsync(p => p.Id.Equals(profile.Id));
            if (isExist)
                throw new InvalidOperationException("Profile with this id already exists.");

            await _context.Profiles.AddAsync(profile);
            return profile;
        }

        public async Task<UserProfile> UpdateProfileAsync(UserProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return profile;
        }
    }
}
