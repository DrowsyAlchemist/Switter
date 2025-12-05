namespace TweetService.Interfaces.Services
{
    public interface ILikeService
    {
        Task<bool> LikeTweetAsync(Guid tweetId, Guid userId);
        Task<bool> UnlikeTweetAsync(Guid tweetId, Guid userId);
        Task<List<Guid>> GetLikedTweetIdsAsync(Guid userId, List<Guid> tweetIds);
    }
}
