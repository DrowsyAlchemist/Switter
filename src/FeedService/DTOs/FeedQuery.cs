namespace FeedService.DTOs
{
    public class FeedQuery
    {
        public required int PageSize { get; set; }
        public string? Cursor { get; set; } // For pagination
    }
}
