namespace FeedService.Models.Options
{
    public class FeedOptions
    {
        public int FollowingsMaxCount = 30;
        public int TweetsByEachFollowingMaxCount = 10;
        public int AllFollowingsTweetsMaxCount = 500;

        public int TrendTweetsMaxCount = 400;

        public int TrendCategoriesMaxCount = 5;
        public int TrendTweetsInCategoryMaxCount = 100;

        public int MaxFeedSize { get; set; } = 1000;
        public int FeedItemsCountForFollowers { get; set; } = 30;
        public TimeSpan FeedTtl { get; set; } = TimeSpan.FromDays(7);
        public int BatchSize { get; set; } = 100;
    }
}
