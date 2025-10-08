using AuthService.DTOs.Jwt;

namespace AuthService.Interfaces.Jwt
{
    internal interface IAccessTokenService
    {
        AccessTokenData GenerateToken(UserClaims user);
        Guid ValidateToken(string token);
    }
}
