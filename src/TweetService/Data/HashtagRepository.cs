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

        public async Task<List<Hashtag>> GetMostPopularAsync(int count)
        {
            try
            {
                return await _context.Hashtags
                       .AsNoTracking()
                       .OrderByDescending(h => h.UsageCount)
                       .Take(count)
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Hashtag?> GetByTagAsync(string tag)
        {
            try
            {
                tag = tag.ToLower();
                return await _context.Hashtags
                       .AsNoTracking()
                       .Where(h => h.Tag.Equals(tag))
                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Hashtag>> GetByTagsAsync(List<string> tags)
        {
            try
            {
                tags = tags.Select(t => t.ToLower()).ToList();
                return await _context.Hashtags
                       .AsNoTracking()
                       .Where(h => tags.Contains(h.Tag))
                       .ToListAsync();
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
                query = query.ToLower();
                if (string.IsNullOrEmpty(query) == false)
                    hashtags = hashtags.Where(h => h.Tag.Contains(query)).ToList();

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
                hashtag.Tag = hashtag.Tag.ToLower();
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
                hashtag.Tag = hashtag.Tag.ToLower();
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
