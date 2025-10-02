using AuthService.DTOs.Jwt;
using AuthService.Models;

namespace AuthService.Interfaces
{
    public interface IAccessTokenService
    {
        AccessTokenData GenerateAccessToken(User user);
        Guid? ValidateAccessToken(string token);
    }
}
