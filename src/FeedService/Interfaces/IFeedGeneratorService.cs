using FeedService.Events;

namespace FeedService.Interfaces
{
    public interface IFeedGeneratorService
    {
        Task ProcessTweetCreatedAsync(TweetCreatedEvent tweetEvent);
        Task ProcessTweetLikedAsync(LikeSetEvent likeEvent);
        Task ProcessUserFollowedAsync(UserFollowedEvent followEvent);
    }
}
