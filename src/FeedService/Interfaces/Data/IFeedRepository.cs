using FeedService.Models;

namespace FeedService.Interfaces.Data
{
    public interface IFeedRepository
    {
        Task AddToFeedAsync(Guid userId, FeedItem item);
        Task AddToFeedAsync(Guid userId, IEnumerable<FeedItem> items);
        Task RemoveUserTweetsFromFeedAsync(Guid feedOwnerId, Guid userToRemoveId);
        Task RemoveFromFeedAsync(Guid userId, Guid tweetId);
        Task<List<FeedItem>> GetFeedPageAsync(Guid userId, int start, int count);
        Task<long> GetFeedLengthAsync(Guid userId);

        Task TrimFeedAsync(Guid userId, long maxLength);
        Task ClearFeedAsync(Guid userId);
        Task<bool> FeedExistsAsync(Guid userId);


        Task IncrementFeedCounterAsync(Guid userId);
        Task<long> GetFeedCounterAsync(Guid userId);
    }
}
