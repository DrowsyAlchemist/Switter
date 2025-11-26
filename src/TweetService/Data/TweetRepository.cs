using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class TweetRepository : ITweetRepository
    {
        private readonly TweetDbContext _context;
        private readonly ILogger<TweetRepository> _logger;

        public TweetRepository(TweetDbContext tweetDbContext, ILogger<TweetRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<Tweet?> GetById(Guid id)
        {
            try
            {
                return await _context.Tweets
                       .AsNoTracking()
                       .Where(t => t.Id.Equals(id))
                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<List<Tweet>> GetByUser(Guid userId)
        {
            try
            {
                return await _context.Tweets
                       .AsNoTracking()
                       .Where(t => t.AuthorId.Equals(userId))
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Tweet> Add(Tweet tweet)
        {
            ArgumentNullException.ThrowIfNull(tweet);
            try
            {
                await _context.Tweets.AddAsync(tweet);
                await _context.SaveChangesAsync();
                return tweet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Tweet> Update(Tweet tweet)
        {
            ArgumentNullException.ThrowIfNull(tweet);
            try
            {
                _context.Tweets.Update(tweet);
                await _context.SaveChangesAsync();
                return tweet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Tweet> Delete(Guid id)
        {
            try
            {
                var tweet = _context.Tweets
                    .AsNoTracking()
                    .Where(t => t.Id.Equals(id))
                    .FirstOrDefault();

                if (tweet == null)
                    throw new ArgumentException(nameof(id));

                _context.Tweets.Remove(tweet);
                await _context.SaveChangesAsync();
                return tweet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }
    }
}
