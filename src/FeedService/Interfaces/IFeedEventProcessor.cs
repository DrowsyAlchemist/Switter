using FeedService.Events;

namespace FeedService.Interfaces
{
    public interface IFeedEventProcessor
    {
        Task ProcessTweetCreatedAsync(TweetCreatedEvent tweetEvent);
        Task ProcessRetweetAsync(RetweetCreatedEvent retweetEvent);
        Task ProcessTweetLikedAsync(LikeSetEvent likeEvent);
        Task ProcessUserFollowedAsync(UserFollowedEvent followEvent);
    }
}
