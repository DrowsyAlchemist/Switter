using FeedService.DTOs;

namespace FeedService.Interfaces
{
    public interface IFeedService
    {
        Task<FeedResponse> GetFeedAsync(Guid userId, FeedQuery query);
        Task<int> GetFeedSizeAsync(Guid userId);

        Task RemoveFromFeedAsync(Guid userId, Guid tweetId);
        Task RemoveUserTweetsFromFeedAsync(Guid feedOwnerId, Guid userToRemoveId);
        Task RebuildFeedAsync(Guid userId);
    }
}
