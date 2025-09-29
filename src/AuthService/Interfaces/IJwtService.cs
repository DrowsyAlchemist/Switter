using AuthService.DTOs.Jwt;
using AuthService.Models;

namespace AuthService.Interfaces
{
    public interface IJwtService
    {
        AccessTokenData GenerateAccessToken(User user);
        RefreshTokenData GenerateRefreshToken();
        Guid? ValidateToken(string token);
    }
}
