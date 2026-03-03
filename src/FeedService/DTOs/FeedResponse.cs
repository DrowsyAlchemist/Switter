namespace FeedService.DTOs
{
    public class FeedResponse
    {
        public List<TweetDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
        public string? NextCursor { get; set; }
    }
}
