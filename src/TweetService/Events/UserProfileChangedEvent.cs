namespace TweetService.Events
{
    public record class UserProfileChangedEvent(Guid UserId, string? DisplayName, string? Bio, string? AvatarUrl);
}
