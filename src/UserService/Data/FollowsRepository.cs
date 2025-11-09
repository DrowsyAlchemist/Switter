using Microsoft.EntityFrameworkCore;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Models;

namespace UserService.Data
{
    public class FollowsRepository : IFollowRepository, IFollowChecker
    {
        private readonly UserDbContext _context;

        public FollowsRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Follow> AddAsync(Guid followerId, Guid followeeId)
        {
            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId
            };

            await _context.Follows.AddAsync(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task<Follow?> GetAsync(Guid followerId, Guid followeeId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

            return follow;
        }

        public async Task DeleteAsync(Guid followerId, Guid followeeId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

            if (follow == null)
                throw new ArgumentException("Follow is not found.");

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserProfile>> GetFollowersAsync(Guid followeeId)
        {
            return await _context.Follows
                   .Where(f => f.FolloweeId == followeeId)
                   .Select(f => f.Follower)
                   .ToListAsync();
        }

        public async Task<List<UserProfile>> GetFollowingsAsync(Guid followerId)
        {
            return await _context.Follows
                    .Where(f => f.FolloweeId == followerId)
                    .Select(f => f.Follower)
                    .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        }
    }
}
