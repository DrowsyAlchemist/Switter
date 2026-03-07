using NotificationService.DTOs;
using NotificationService.Interfaces.Infrastructure;
using System.Text.Json;

namespace NotificationService.Services.Infrastructure
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _userProfileUrl;
        private readonly string _userFollowersUrl;
        private readonly ILogger<ProfileServiceClient> _logger;

        public ProfileServiceClient(HttpClient httpClient, ILogger<ProfileServiceClient> logger)
        {
            _httpClient = httpClient;
            _userProfileUrl = "/api/users/";
            _userFollowersUrl = "api/follow/followers/";
            _logger = logger;
        }

        public async Task<UserInfo?> GetUserInfoAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_userProfileUrl}{userId}");

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
        public async Task<List<Guid>> GetFollowersIds(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_userFollowersUrl}{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var followers = JsonSerializer.Deserialize<IEnumerable<UserInfo>>(content);
                    if (followers == null)
                        throw new JsonException($"Failed to deserialize followers for user {userId}");
                    return followers.Select(f => f.Id).ToList();
                }
                _logger.LogError("Failed to get user info for {UserId}: {StatusCode}", userId, response.StatusCode);
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info for {UserId}", userId);
                return [];
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
