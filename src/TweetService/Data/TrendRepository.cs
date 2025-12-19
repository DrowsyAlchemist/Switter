using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class TrendRepository : ITrendRepository
    {
        private const string ErrorMessage = "Db is unavailable";
        private readonly TweetDbContext _context;
        private readonly ILogger<TrendRepository> _logger;

        public TrendRepository(TweetDbContext tweetDbContext, ILogger<TrendRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<List<Tweet>> GetMostLikedAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                return await _context.Tweets
                    .OrderByDescending(t => t.LikesCount)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Tweet>> GetMostLikedAsync(string hashtag, int page = 1, int pageSize = 10)
        {
            try
            {
                hashtag = hashtag.ToLower();
                return await _context.TweetHashtags
                    .Where(th => th.Hashtag.Tag.Equals(hashtag))
                    .Select(th => th.Tweet)
                    .OrderByDescending(t => t.LikesCount)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Tweet>> GetMostRetweetedAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                return await _context.Tweets
                    .OrderByDescending(t => t.RetweetsCount)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Tweet>> GetMostRetweetedAsync(string hashtag, int page = 1, int pageSize = 10)
        {
            try
            {
                hashtag = hashtag.ToLower();
                return await _context.TweetHashtags
                    .Where(th => th.Hashtag.Tag.Equals(hashtag))
                    .Select(th => th.Tweet)
                    .OrderByDescending(t => t.RetweetsCount)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }
    }
}
