using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationSettingsController : ControllerBase
    {
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly ILogger<NotificationSettingsController> _logger;

        public NotificationSettingsController(
            INotificationSettingsService notificationSettingsService,
            ILogger<NotificationSettingsController> logger)
        {
            _notificationSettingsService = notificationSettingsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("GetSettings failed. Current user not found.");
                    return Unauthorized();
                }

                var notifications = await _notificationSettingsService.GetSettingsAsync(userId.Value);
                _logger.LogInformation("Settings successfully sent for user {id}.", userId.Value);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetSettings.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UserNotificationSettingsDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("UpdateSettings failed. Current user not found.");
                    return Unauthorized();
                }

                var notifications = await _notificationSettingsService.UpdateSettingsAsync(dto);
                _logger.LogInformation("Settings successfully updated for user {id}.", userId.Value);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during UpdateSettings.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || Guid.TryParse(userIdStr, out Guid userId) == false)
                return null;
            return userId;
        }
    }
}
