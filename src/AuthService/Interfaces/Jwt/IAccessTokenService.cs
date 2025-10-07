using AuthService.DTOs.Jwt;

namespace AuthService.Interfaces.Jwt
{
    internal interface IAccessTokenService
    {
        AccessTokenData GenerateAccessToken(UserClaims user);
        Guid? ValidateAccessToken(string token);
    }
}
