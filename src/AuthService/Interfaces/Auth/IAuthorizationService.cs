using AuthService.DTOs.Auth;

namespace AuthService.Interfaces.Auth
{
    public interface IAuthorizationService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, string remoteIp);
        Task<AuthResponse> LoginAsync(LoginRequest request, string remoteIp);
        Task<AuthResponse> RefreshTokenAsync(RefreshRequest request, string remoteIp);
        Task RevokeTokenAsync(string refreshToken, string remoteIp);
    }
}
