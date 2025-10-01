using AuthService.DTOs;

namespace AuthService.Interfaces
{
    public interface IAuthorizationService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, string remoteIp);
        Task<AuthResponse> LoginAsync(LoginRequest request, string remoteIp);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string remoteIp);
        Task RevokeTokenAsync(string refreshToken, string remoteIp);
    }
}
