using Microsoft.EntityFrameworkCore;
using UserService.Interfaces.Data;
using UserService.Models;

namespace UserService.Data
{
    public class FollowsRepository : IFollowRepository
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
                .AsNoTracking()
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

        public async Task<IEnumerable<UserProfile>> GetFollowersAsync(Guid followeeId, int page, int pageSize)
        {
            return await _context.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == followeeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserProfile>> GetFollowingsAsync(Guid followerId, int page, int pageSize)
        {
            return await _context.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Followee)
                .ToListAsync();
        }

        public async Task<IEnumerable<Guid>> GetFollowerIdsAsync(Guid followeeId, int page, int pageSize)
        {
            return await _context.Follows
                .Where(f => f.FolloweeId == followeeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Guid>> GetFollowingIdsAsync(Guid followerId, int page, int pageSize)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == followerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Followee.Id)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        }
    }
}
