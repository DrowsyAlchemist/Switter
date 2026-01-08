using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class LikeRepository : ILikesRepository
    {
        private readonly TweetDbContext _context;

        public LikeRepository(TweetDbContext tweetDbContext)
        {
            _context = tweetDbContext;
        }

        public async Task<Like?> GetByIdAsync(Guid id)
        {
            return await _context.Likes
                   .AsNoTracking()
                   .Where(l => l.Id.Equals(id))
                   .FirstOrDefaultAsync();
        }

        public async Task<List<Like>> GetByUserAsync(Guid userId)
        {
            return await _context.Likes
                   .AsNoTracking()
                   .Where(l => l.UserId.Equals(userId))
                   .Include(l => l.Tweet)
                   .ToListAsync();
        }

        public async Task<Like?> GetAsync(Guid tweetId, Guid userId)
        {
            return await _context.Likes
                   .AsNoTracking()
                   .Where(l => l.UserId.Equals(userId)
                        && l.TweetId.Equals(tweetId))
                   .FirstOrDefaultAsync();
        }

        public async Task<List<Guid>> GetLikedTweetIdsAsync(List<Guid> tweetIds, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(tweetIds);
            if (tweetIds.Count == 0)
                return new List<Guid>();

            return await _context.Likes
                       .AsNoTracking()
                       .Where(l => l.UserId.Equals(userId)
                            && tweetIds.Contains(l.TweetId))
                       .Select(l => l.TweetId)
                       .ToListAsync();
        }

        public async Task<List<Guid>> GetLikedTweetIdsAsync(Guid userId)
        {
            return await _context.Likes
                   .AsNoTracking()
                   .Where(l => l.UserId.Equals(userId))
                   .Select(l => l.TweetId)
                   .ToListAsync();
        }

        public async Task<Like> AddAsync(Like like)
        {
            ArgumentNullException.ThrowIfNull(like);
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
            _context.Entry(like).State = EntityState.Detached;
            return like;
        }

        public async Task DeleteAsync(Guid id)
        {
            var like = _context.Likes
                .Where(l => l.Id.Equals(id))
                .FirstOrDefault();

            if (like == null)
                throw new ArgumentException(nameof(id));

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsExistAsync(Guid tweetId, Guid userId)
        {
            return await _context.Likes.AnyAsync(l => l.UserId == userId && l.TweetId == tweetId);
        }
    }
}
