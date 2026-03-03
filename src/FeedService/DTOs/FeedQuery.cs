namespace FeedService.DTOs
{
    public class FeedQuery
    {
        public required int PageSize { get; set; }
        public int Cursor { get; set; } = 0;
    }
}
