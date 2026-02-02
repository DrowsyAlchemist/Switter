namespace FeedService.DTOs
{
    public class FeedResponse
    {
        public List<FeedItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
        public string? NextPageToken { get; set; }
    }
}
