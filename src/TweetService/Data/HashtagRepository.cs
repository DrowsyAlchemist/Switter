using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class HashtagRepository : IHashtagRepository
    {
        private const string ErrorMessage = "Db is unavailable";
        private readonly TweetDbContext _context;
        private readonly ILogger<HashtagRepository> _logger;

        public HashtagRepository(TweetDbContext tweetDbContext, ILogger<HashtagRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<Hashtag?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Hashtags
                       .AsNoTracking()
                       .Where(h => h.Id.Equals(id))
                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Hashtag>> SearchAsync(string query, int page, int pageSize)
        {
            try
            {
                var hashtags = await _context.Hashtags
                                        .AsNoTracking()
                                        .ToListAsync();

                if (string.IsNullOrEmpty(query) == false)
                    hashtags = hashtags.Where(h => h.Tag.ToLower().Contains(query.ToLower())).ToList();

                return hashtags
                        .Skip(pageSize * (page - 1))
                        .Take(pageSize)
                        .ToList();
            }
            catch (Exception ex)

            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Hashtag> AddAsync(Hashtag hashtag)
        {
            ArgumentNullException.ThrowIfNull(hashtag);
            try
            {
                await _context.Hashtags.AddAsync(hashtag);
                await _context.SaveChangesAsync();
                return hashtag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Hashtag> UpdateAsync(Hashtag hashtag)
        {
            ArgumentNullException.ThrowIfNull(hashtag);
            try
            {
                _context.Hashtags.Update(hashtag);
                await _context.SaveChangesAsync();
                return hashtag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Hashtag> DeleteAsync(Guid id)
        {
            try
            {
                var hashtag = _context.Hashtags
                    .AsNoTracking()
                    .Where(h => h.Id.Equals(id))
                    .FirstOrDefault();

                if (hashtag == null)
                    throw new ArgumentException(nameof(id));

                _context.Hashtags.Remove(hashtag);
                await _context.SaveChangesAsync();
                return hashtag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }
    }
}
