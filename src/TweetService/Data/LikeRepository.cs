using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class LikeRepository : ILikesRepository
    {
        private const string ErrorMessage = "Db is unavailable";
        private readonly TweetDbContext _context;
        private readonly ILogger<LikeRepository> _logger;

        public LikeRepository(TweetDbContext tweetDbContext, ILogger<LikeRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<Like?> GetByIdAsync(Guid id)
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
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Like>> GetByUserAsync(Guid userId)
        {
            try
            {
                return await _context.Likes
                       .AsNoTracking()
                       .Where(l => l.UserId.Equals(userId))
                       .Include(l => l.Tweet)
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Like?> GetAsync(Guid tweetId, Guid userId)
        {
            try
            {
                return await _context.Likes
                       .AsNoTracking()
                       .Where(l => l.UserId.Equals(userId)
                            && l.TweetId.Equals(tweetId))
                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Guid>> GetLikedTweetIdsAsync(List<Guid> tweetIds, Guid userId)
        {
            try
            {
                return await _context.Likes
                       .AsNoTracking()
                       .Where(l => l.UserId.Equals(userId)
                            && tweetIds.Contains(l.TweetId))
                       .Select(l => l.TweetId)
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Guid>> GetLikedTweetIdsAsync(Guid userId)
        {
            try
            {
                return await _context.Likes
                       .AsNoTracking()
                       .Where(l => l.UserId.Equals(userId))
                       .Select(l => l.TweetId)
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Like> AddAsync(Like like)
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
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Like> DeleteAsync(Guid id)
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
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<bool> IsExistAsync(Guid tweetId, Guid userId)
        {
            try
            {
                return await _context.Likes.AnyAsync(l => l.UserId == userId && l.TweetId == tweetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }
    }
}
