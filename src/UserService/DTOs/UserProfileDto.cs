namespace UserService.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Для текущего пользователя
        public bool IsFollowing { get; set; } 
        public bool IsFollowed { get; set; } 
        public bool IsBlocking { get; set; }
        public bool IsBlocked { get; set; }
    }
}
