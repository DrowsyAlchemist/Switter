using AuthService.Data;
using AuthService.DTOs;
using AuthService.Interfaces;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly AuthDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(AuthDbContext context, IJwtService jwtService,
                          IUserService userService, ILogger<AuthorizationService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userService.UserExistsAsync(request.Email, request.Username))
                throw new ArgumentException("User with this email or username already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered: {Username} ({Email})", user.Username, user.Email);

            return await GenerateAuthResponseAsync(user, request.RemoteIp);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userService.GetUserByEmailAsync(request.Login)
                  ?? await _userService.GetUserByUsernameAsync(request.Login);

            if (user == null)
                throw new UnauthorizedAccessException($"User with login {request.Login} in not found");

            if (BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash) == false)
                throw new UnauthorizedAccessException("Invalid password");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");

            _logger.LogInformation("User logged in: {Username}", user.Username);
            return await GenerateAuthResponseAsync(user, request.RemoteIp);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var userId = _jwtService.ValidateAccessToken(request.AccessToken);
            if (userId == null)
                throw new SecurityTokenException("Invalid access token");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
                throw new ArgumentException("User not found");

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);
            if (refreshToken == null)
                throw new SecurityTokenException("Refresh token not found");
            if (refreshToken.IsExpired)
                throw new SecurityTokenException("Refresh token expired");
            if (refreshToken.IsRevoked)
                throw new SecurityTokenException("Refresh token revoked");

            var authResponse = await GenerateAuthResponseAsync(user, request.RemoteIp);

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = request.RemoteIp;
            refreshToken.ReplacedByToken = authResponse.RefreshToken;

            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();

            return authResponse;
        }

        public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null || !token.IsActive)
                throw new ArgumentException("Invalid token");

            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;

            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string ipAddress)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken.Token,
                Expires = refreshToken.Expires,
                CreatedByIp = ipAddress,
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = accessToken.Expires
            };
        }
    }
}
