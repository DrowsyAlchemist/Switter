namespace FeedService.Models.Options
{
    public class FeedOptions
    {
        public int MaxFeedSize { get; set; } = 500;
        public TimeSpan FeedTtl { get; set; } = TimeSpan.FromDays(7);
        public int BatchSize { get; set; } = 100;
    }
}
