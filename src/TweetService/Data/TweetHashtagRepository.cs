using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class TweetHashtagRepository : ITweetHashtagRepository
    {
        private const string ErrorMessage = "Db is unavailable";
        private readonly TweetDbContext _context;
        private readonly ILogger<TweetHashtagRepository> _logger;

        public TweetHashtagRepository(TweetDbContext tweetDbContext, ILogger<TweetHashtagRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<List<TweetHashtag>> GetByHashtagAsync(string tag)
        {
            try
            {
                return await _context.TweetHashtags
                       .Include(th => th.Hashtag)
                       .Include(th => th.Tweet)
                       .AsNoTracking()
                       .Where(th => th.Hashtag.Tag.Equals(tag.ToLower()))
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public int GetUsageCount(string tag, TimeSpan period)
        {
            try
            {
                if (string.IsNullOrEmpty(tag))
                    throw new ArgumentException(nameof(tag));

                DateTime startDate = DateTime.UtcNow - period;

                return _context.TweetHashtags
                    .Include(th => th.Tweet)
                    .Where(th => th.Tweet.CreatedAt > startDate).Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<TweetHashtag> AddAsync(TweetHashtag tweetHashtag)
        {
            ArgumentNullException.ThrowIfNull(tweetHashtag);
            try
            {
                await _context.TweetHashtags.AddAsync(tweetHashtag);
                await _context.SaveChangesAsync();
                return tweetHashtag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<TweetHashtag>> AddRangeAsync(List<TweetHashtag> tweetHashtags)
        {
            ArgumentNullException.ThrowIfNull(tweetHashtags);
            try
            {
                await _context.TweetHashtags.AddRangeAsync(tweetHashtags);
                await _context.SaveChangesAsync();
                return tweetHashtags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }
    }
}
