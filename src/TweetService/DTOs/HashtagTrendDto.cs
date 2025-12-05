namespace TweetService.DTOs
{
    public class HashtagTrendDto
    {
        public string Tag { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public int TrendChange { get; set; } 
    }
}
