namespace AuthService.DTOs.Jwt
{
    public class UserClaims
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
