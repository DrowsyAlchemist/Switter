using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class HashtagRepository : IHashtagRepository
    {
        private readonly TweetDbContext _context;

        public HashtagRepository(TweetDbContext tweetDbContext)
        {
            _context = tweetDbContext;
        }

        public async Task<List<Hashtag>> GetMostPopularAsync(int count)
        {
            return await _context.Hashtags
                   .AsNoTracking()
                   .OrderByDescending(h => h.UsageCount)
                   .Take(count)
                   .ToListAsync();
        }

        public async Task<Hashtag?> GetByTagAsync(string tag)
        {
            tag = tag.ToLower();
            return await _context.Hashtags
                   .AsNoTracking()
                   .Where(h => h.Tag.Equals(tag))
                   .FirstOrDefaultAsync();
        }

        public async Task<List<Hashtag>> GetByTagsAsync(List<string> tags)
        {
            ArgumentNullException.ThrowIfNull(tags);
            if (tags.Count == 0)
                return new List<Hashtag>();

            tags = tags.Select(t => t.ToLower()).ToList();
            return await _context.Hashtags
                   .AsNoTracking()
                   .Where(h => tags.Contains(h.Tag))
                   .ToListAsync();
        }

        public async Task<List<Guid>> GetIdByTag(List<string> tags)
        {
            ArgumentNullException.ThrowIfNull(tags);
            if (tags.Count == 0)
                return new List<Guid>();

            tags = tags.Select(t => t.ToLower()).ToList();
            return await _context.Hashtags
                   .AsNoTracking()
                   .Where(h => tags.Contains(h.Tag))
                   .Select(h => h.Id)
                   .ToListAsync();
        }

        public async Task<List<Hashtag>> SearchAsync(string query, int page, int pageSize)
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

        public async Task<Hashtag> AddAsync(Hashtag hashtag)
        {
            ArgumentNullException.ThrowIfNull(hashtag);
            hashtag.Tag = hashtag.Tag.ToLower();

            bool exists = await _context.Hashtags.AnyAsync(h => h.Tag.Equals(hashtag.Tag));
            if (exists)
                throw new ArgumentException($"Hashtag '{hashtag.Tag}' already exists");

            await _context.Hashtags.AddAsync(hashtag);
            await _context.SaveChangesAsync();
            _context.Entry(hashtag).State = EntityState.Detached;
            return hashtag;
        }

        public async Task AddRangeAsync(List<string> hashtags)
        {
            ArgumentNullException.ThrowIfNull(hashtags);
            if (hashtags.Count == 0)
                return;

            hashtags = hashtags.Select(h => h.ToLower()).ToList();

            var existingTags = _context.Hashtags.Where(h => hashtags.Contains(h.Tag));

            if (existingTags.Any())
                throw new ArgumentException($"Hashtags '{string.Join(", ", existingTags)}' already exists");

            var newHashtags = new List<Hashtag>();
            foreach (var tag in hashtags)
            {
                var hashtag = new Hashtag() { Tag = tag };
                newHashtags.Add(hashtag);
            }
            await _context.Hashtags.AddRangeAsync(newHashtags);
            await _context.SaveChangesAsync();
        }

        public async Task<Hashtag> IncrementUsageCounterAsync(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException(nameof(tag));

            tag = tag.ToLower();
            var localTag = await _context.Hashtags.FirstOrDefaultAsync(t => t.Tag.Equals(tag));
            if (localTag == null)
                throw new KeyNotFoundException($"Hashtag #{tag} not found.");

            localTag.UsageCount++;
            await _context.SaveChangesAsync();
            _context.Hashtags.Entry(localTag).State = EntityState.Detached;
            return localTag;
        }

        public async Task IncrementUsageCounterAsync(List<string> tags)
        {
            ArgumentNullException.ThrowIfNull(tags);
            if (tags.Count == 0)
                return;

            tags = tags
                .Select(t => t.ToLower())
                .ToList();

            var existingTags = await _context.Hashtags
                .Where(t => tags.Contains(t.Tag))
                .ToListAsync();

            var existingTagSet = new HashSet<string>(existingTags.Select(t => t.Tag));

            var missingTags = tags.Except(existingTagSet);
            if (missingTags.Any())
                throw new KeyNotFoundException($"Hashtags #{string.Join(", #", missingTags)} not found.");

            foreach (var tag in existingTags)
                tag.UsageCount++;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsExist(string tag)
        {
            return await _context.Hashtags.AnyAsync(t => t.Tag.Equals(tag.ToLower()));
        }

        public async Task<List<string>> GetExists(List<string> tags)
        {
            ArgumentNullException.ThrowIfNull(tags);
            if (tags.Count == 0)
                return new List<string>();

            tags = tags.Select(t => t.ToLower()).ToList();
            return await _context.Hashtags.Where(t => tags.Contains(t.Tag)).Select(t => t.Tag).ToListAsync();
        }
    }
}
