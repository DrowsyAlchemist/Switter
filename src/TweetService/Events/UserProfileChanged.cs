namespace TweetService.Events
{
    public record class UserProfileChanged(Guid UserId, string? DisplayName, string? Bio, string? AvatarUrl);
}
