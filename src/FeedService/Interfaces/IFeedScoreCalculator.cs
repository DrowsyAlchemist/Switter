namespace FeedService.Interfaces
{
    public interface IFeedScoreCalculator
    {
        double Calculate(DateTime createdAt, int likes, int retweets);
    }
}
