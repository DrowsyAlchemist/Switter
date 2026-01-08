using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class TweetHashtagRepository : ITweetHashtagRepository
    {
        private readonly TweetDbContext _context;

        public TweetHashtagRepository(TweetDbContext tweetDbContext)
        {
            _context = tweetDbContext;
        }

        public async Task<List<TweetHashtag>> GetByHashtagAsync(string tag)
        {
            return await _context.TweetHashtags
                   .Include(th => th.Hashtag)
                   .Include(th => th.Tweet)
                   .AsNoTracking()
                   .Where(th => th.Hashtag.Tag.Equals(tag.ToLower()))
                   .ToListAsync();
        }

        public int GetUsageCount(string tag, TimeSpan period)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException(nameof(tag));

            DateTime startDate = DateTime.UtcNow - period;

            return _context.TweetHashtags
                .Include(th => th.Tweet)
                .Where(th => th.Tweet.CreatedAt > startDate)
                .Count();
        }

        public async Task<TweetHashtag> AddAsync(TweetHashtag tweetHashtag)
        {
            ArgumentNullException.ThrowIfNull(tweetHashtag);
            await _context.TweetHashtags.AddAsync(tweetHashtag);
            await _context.SaveChangesAsync();
            return tweetHashtag;
        }

        public async Task<List<TweetHashtag>> AddRangeAsync(List<TweetHashtag> tweetHashtags)
        {
            ArgumentNullException.ThrowIfNull(tweetHashtags);
            if (tweetHashtags.Count == 0)
                return tweetHashtags;

            await _context.TweetHashtags.AddRangeAsync(tweetHashtags);
            await _context.SaveChangesAsync();
            return tweetHashtags;
        }
    }
}
