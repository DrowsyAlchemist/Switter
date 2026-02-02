namespace FeedService.DTOs
{
    public class FeedQuery
    {
        public int PageSize { get; set; } = 20;
        public string? Cursor { get; set; } // For pagination
        public FeedType Type { get; set; } = FeedType.Home;
    }
}
