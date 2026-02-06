using FeedService.Events;

namespace FeedService.Interfaces
{
    public interface IFeedEventProcessor
    {
        Task ProcessTweetCreatedAsync(TweetCreatedEvent tweetEvent);
        Task ProcessRetweetAsync(RetweetCreatedEvent retweetEvent);
        Task ProcessTweetLikedAsync(LikeSetEvent likeEvent);
        Task ProcessUserFollowedAsync(UserFollowedEvent followEvent);

        Task ProcessUserUnfollowedAsync(UserUnfollowedEvent userUnfollowedEvent);
        Task ProcessUserBlockedAsync(UserBlockedEvent userBlockedEvent);
    }
}
