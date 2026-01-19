namespace TweetService.Interfaces.Services
{
    public interface IHashtagService
    {
        Task<IEnumerable<string>> ProcessHashtagsAsync(Guid tweetId, string content);
    }
}
