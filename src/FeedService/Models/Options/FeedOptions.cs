namespace FeedService.Models.Options
{
    public class FeedOptions
    {
        public required int MaxFeedSize { get; set; }

        public required int FollowingsMaxCount { get; set; }
        public required int TweetsByEachFollowingMaxCount { get; set; }
        public required int AllFollowingsTweetsMaxCount { get; set; }

        public required int TrendTweetsMaxCount { get; set; }

        public required int TrendCategoriesMaxCount { get; set; }
        public required int TrendTweetsInCategoryMaxCount { get; set; }

        public required int FeedTtlInHours { get; set; }

        public required ScoreOptions Score { get; set; }
    }

    public class ScoreOptions
    {
        public required double LikeWeight { get; set; }
        public required double RetweetWeight { get; set; }
        public required int MaxNoveltyFactor { get; set; }
        public required int NoveltyFactorExpiryInHours { get; set; }
        public required double TimeDecayFactor { get; set; }
    }
}
