namespace FeedService.Interfaces
{
    public interface IFeedFiller
    {
        Task AddTweetToFeedAsync(Guid tweetId, Guid userId);
        Task AddTweetsToFeedAsync(IEnumerable<Guid> tweetIds, Guid userId);
        Task AddTweetToFeedsAsync(Guid tweetId, IEnumerable<Guid> userIds);

        Task RemoveUserTweetsFromFeed(Guid feedOwnerId, Guid userToRemoveId);
    }
}
