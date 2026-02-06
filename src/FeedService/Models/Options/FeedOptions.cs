namespace FeedService.Models.Options
{
    public class FeedOptions
    {
        public int MaxFeedSize { get; set; }

        public int FollowingsMaxCount { get; set; }
        public int TweetsByEachFollowingMaxCount { get; set; }
        public int AllFollowingsTweetsMaxCount { get; set; }

        public int TrendTweetsMaxCount { get; set; }

        public int TrendCategoriesMaxCount { get; set; }
        public int TrendTweetsInCategoryMaxCount { get; set; }

        public int FeedTtlInHours { get; set; }

        public ScoreOptions? Score { get; set; }
    }

    public class ScoreOptions
    {
        public double LikeWeight { get; set; }
        public double RetweetWeight { get; set; }
        public int MaxNoveltyFactor { get; set; }
        public int NoveltyFactorExpiryInHours { get; set; }
        public double TimeDecayFactor { get; set; }
    }
}
