namespace TweetService.Interfaces.Services
{
    public interface IHashtagService
    {
        Task ProcessHashtagsAsync(Guid tweetId);
    }
}
