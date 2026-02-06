namespace FeedService.Interfaces
{
    public interface IFeedFiller
    {
        Task AddTweetToFeedAsync(Guid tweetId, Guid userId);
        Task AddTweetsToFeedAsync(List<Guid> tweetIds, Guid userId);
        Task AddTweetToFeedsAsync(Guid tweetId, IEnumerable<Guid> userIds);

        Task RemoveUserTweetsFromFeedAsync(Guid feedOwnerId, Guid userToRemoveId);
        Task ClearFeedAsync(Guid userId);
    }
}
