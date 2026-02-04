using FeedService.Models;

namespace FeedService.Interfaces.Data
{
    public interface IFeedRepository
    {
        Task AddToFeedAsync(Guid userId, FeedItem item);
        Task AddToFeedAsync(Guid userId, List<FeedItem> items);
        Task RemoveUserTweetsFromFeedAsync(Guid feedOwnerId, Guid userToRemoveId);
        Task RemoveFromFeedAsync(Guid userId, Guid tweetId);
        Task ClearFeedAsync(Guid userId);

        Task<List<FeedItem>> GetFeedPageAsync(Guid userId, int start, int count);
        Task<long> GetFeedLengthAsync(Guid userId);
    }
}
