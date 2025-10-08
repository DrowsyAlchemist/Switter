using AuthService.DTOs.Jwt;

namespace AuthService.Interfaces.Jwt
{
    internal interface IAccessTokenService
    {
        AccessTokenData GenerateToken(UserClaims user);
        ValidateTokenResult ValidateToken(string token);
    }
}
