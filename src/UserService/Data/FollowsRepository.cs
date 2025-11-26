using Microsoft.EntityFrameworkCore;
using UserService.Interfaces.Data;
using UserService.Models;

namespace UserService.Data
{
    public class FollowsRepository : IFollowRepository
    {
        private readonly UserDbContext _context;
        private readonly ILogger<FollowsRepository> _logger;

        public FollowsRepository(UserDbContext context, ILogger<FollowsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Follow> AddAsync(Guid followerId, Guid followeeId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Follow?> GetAsync(Guid followerId, Guid followeeId)
        {
            try
            {
                var follow = await _context.Follows
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
                return follow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task DeleteAsync(Guid followerId, Guid followeeId)
        {
            try
            {
                var follow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

                if (follow == null)
                    throw new ArgumentException("Follow is not found.");

                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<List<UserProfile>> GetFollowersAsync(Guid followeeId)
        {
            try
            {
                return await _context.Follows
                    .AsNoTracking()
                    .Where(f => f.FolloweeId == followeeId)
                    .Select(f => f.Follower)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<List<UserProfile>> GetFollowingsAsync(Guid followerId)
        {
            try
            {
                return await _context.Follows
                    .AsNoTracking()
                    .Where(f => f.FolloweeId == followerId)
                    .Select(f => f.Follower)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            try
            {
                return await _context.Follows
                    .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }
    }
}
