namespace FeedService.Interfaces
{
    public interface IScoreCalculator
    {
        Task<double> CalculateAsync(Guid tweetId);
    }
}
