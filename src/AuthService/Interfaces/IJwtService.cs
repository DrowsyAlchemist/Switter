using AuthService.Models;

namespace AuthService.Interfaces
{
    interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Guid? ValidateToken(string token);

    }
}
