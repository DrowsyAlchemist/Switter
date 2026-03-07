using Microsoft.Extensions.Options;
using System.Text.Json;
using TweetService.DTOs;
using TweetService.Interfaces.Infrastructure;
using TweetService.Models.Options;

namespace TweetService.Services.Infrastructure
{
    public class UserServiceClient : IUserServiceClient
    {
        private const string GetUserProfileEndpoint = "/api/UserProfile";
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;
        private readonly string _getUserProfileUrl;

        public UserServiceClient(HttpClient httpClient, IOptions<AppUrls> appUrls, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _getUserProfileUrl = appUrls.Value.UserServiceUrl + GetUserProfileEndpoint;
            _logger = logger;
        }

        public async Task<UserInfo?> GetUserInfoAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_getUserProfileUrl}/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserInfo>(content);
                }
                _logger.LogWarning("Failed to get user info for {UserId}: {StatusCode}", userId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info for {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/ping");
                var content = await response.Content.ReadAsStringAsync();
                return content != null && content == "pong";
            }
            catch
            {
                return false;
            }
        }
    }
}