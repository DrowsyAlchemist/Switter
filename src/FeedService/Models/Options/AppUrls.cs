namespace FeedService.Models.Options
{
    public class AppUrls
    {
        public required string AuthServiceUrl { get; set; }
        public required string UserServiceUrl { get; set; }
        public required string TweetServiceUrl { get; set; }
        public required string FeedServiceUrl { get; set; }
        public required string NotificationServiceUrl { get; set; }
    }
}