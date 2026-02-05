using FeedService.Interfaces;

namespace FeedService.Services
{
    public class FeedScoreCalculator : IFeedScoreCalculator
    {
        private const double LikeWeight = 1;
        private const double RetweetWeight = 2;

        private const int MaxNoveltyFactor = 1000;
        private const int NoveltyFactorExpiryInHours = 48;

        private const double TimeDecayFactor = 0.85f;

        private const int HoursInDay = 24;

        public double Calculate(DateTime createdAt, int likes, int retweets)
        {
            var hoursSinceCreation = (DateTime.UtcNow - createdAt).TotalHours;
            var novelty = MaxNoveltyFactor * Math.Max(0, (1 - hoursSinceCreation / NoveltyFactorExpiryInHours));

            var engagement = likes * LikeWeight + retweets * RetweetWeight;

            var total = novelty + engagement;

            var timeCoefficient = hoursSinceCreation > NoveltyFactorExpiryInHours
                ? Math.Pow(TimeDecayFactor, (hoursSinceCreation - NoveltyFactorExpiryInHours) / HoursInDay)
                : 1.0;

            return total * timeCoefficient;
        }
    }
}
