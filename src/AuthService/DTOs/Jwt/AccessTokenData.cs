namespace AuthService.DTOs.Jwt
{
    public class AccessTokenData
    {
        public required string Token {  get; set; }
        public required DateTime Expires { get; set; }
    }
}
