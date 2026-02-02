using FeedService.DTOs;
using FeedService.Models;

namespace FeedService.Interfaces
{
    public interface IFeedService
    {
        Task<FeedResponse> GetFeedAsync(Guid userId, FeedQuery query);
        Task<int> GetFeedSizeAsync(Guid userId);

        Task<bool> AddToFeedAsync(Guid userId, FeedItem item);
        Task<bool> RemoveFromFeedAsync(Guid userId, Guid tweetId);
        Task RebuildFeedAsync(Guid userId);
    }
}
