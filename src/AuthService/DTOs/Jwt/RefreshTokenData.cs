namespace AuthService.DTOs.Jwt
{
    public class RefreshTokenData
    {
        public required string Token { get; set; }
        public required DateTime Expires {  get; set; }

    }
}
