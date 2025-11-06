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

        public async Task<UserProfile> GetProfileAsync(Guid userId)
        {
            var profileInDb = await _context.Profiles
                .Where(p => p.Id == userId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (profileInDb == null)
                throw new ArgumentException("User profile with id {id} is not found.", userId.ToString());

            return profileInDb;
        }

        public async Task<UserProfile> UpdateProfileAsync(UserProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);
            var profileInDb = await _context.Profiles.Where(p => p.Id == profile.Id).FirstOrDefaultAsync();
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return profile;
        }
    }
}
