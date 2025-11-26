using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class LikeRepository : ILikesRepository
    {
        private readonly TweetDbContext _context;
        private readonly ILogger<LikeRepository> _logger;

        public LikeRepository(TweetDbContext tweetDbContext, ILogger<LikeRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<Like?> GetById(Guid id)
        {
            try
            {
                return await _context.Likes
                       .AsNoTracking()
                       .Where(l => l.Id.Equals(id))
                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<List<Like>> GetByUser(Guid userId)
        {
            try
            {
                return await _context.Likes
                       .AsNoTracking()
                       .Where(t => t.UserId.Equals(userId))
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Like> Add(Like like)
        {
            ArgumentNullException.ThrowIfNull(like);
            try
            {
                await _context.Likes.AddAsync(like);
                await _context.SaveChangesAsync();
                return like;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Like> Delete(Guid id)
        {
            try
            {
                var like = _context.Likes
                    .AsNoTracking()
                    .Where(l => l.Id.Equals(id))
                    .FirstOrDefault();

                if (like == null)
                    throw new ArgumentException(nameof(id));

                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
                return like;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }
    }
}
