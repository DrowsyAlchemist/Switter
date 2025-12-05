namespace TweetService.DTOs
{
    public class UserInfo
    {
        public required Guid Id { get; set; }
        public required string DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
