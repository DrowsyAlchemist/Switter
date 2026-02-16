using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Exceptions;
using NotificationService.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService,
                                     ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (IsPaginationValid(page, pageSize) == false)
            {
                _logger.LogWarning("GetNotifications failed. Pagination is invalid. Page: {page}; PageSize: {size}", page, pageSize);
                return BadRequest();
            }
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("GetNotifications failed. Current user not found.");
                    return Unauthorized();
                }

                var notifications = await _notificationService.GetNotificationsAsync(userId.Value, page, pageSize);
                _logger.LogInformation("{Count} notifications successfully sent.", notifications.Count);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetNotifications.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (IsPaginationValid(page, pageSize) == false)
            {
                _logger.LogWarning("GetNotifications failed. Pagination is invalid. Page: {page}; PageSize: {size}", page, pageSize);
                return BadRequest();
            }
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("GetUnreadNotifications failed. Current user not found.");
                    return Unauthorized();
                }

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId.Value, page, pageSize);
                _logger.LogInformation("{Count} unread notifications successfully sent.", notifications.Count);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetUnreadNotifications.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("GetUnreadCount failed. Current user not found.");
                    return Unauthorized();
                }

                int count = await _notificationService.GetUnreadCountAsync(userId.Value);
                _logger.LogInformation("GetUnreadCount successfully sent. ({Count})", count);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetUnreadCount.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("MarkAsRead failed. Current user not found.");
                    return Unauthorized();
                }

                await _notificationService.MarkAsReadAsync(notificationId, userId.Value);
                _logger.LogInformation("Notification {id} successfully marked as read.", notificationId);
                return Ok();
            }
            catch (NotificationAlreadyReadException ex)
            {
                _logger.LogWarning(ex, "MarkAsRead failed. Notification has already read.");
                return Ok();
            }
            catch (NotificationNotFoundException ex)
            {
                _logger.LogWarning(ex, "MarkAsRead failed. Notification not found. Id: {id}", notificationId);
                return NotFound();
            }
            catch (NotificationOwnerNotMatchException ex)
            {
                _logger.LogWarning(ex, "MarkAsRead failed. User does not match with notification owner.");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MarkAsRead.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("read/all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("MarkAllAsRead failed. Current user not found.");
                    return Unauthorized();
                }

                var count = await _notificationService.MarkAllAsReadAsync(userId.Value);
                _logger.LogInformation("{Count} notifications successfully marked as read.", count);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MarkAllAsRead.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("DeleteNotification failed. Current user not found.");
                    return Unauthorized();
                }

                await _notificationService.DeleteNotificationAsync(notificationId, userId.Value);
                _logger.LogInformation("Notification {id} successfully removed.", notificationId);
                return Ok();
            }
            catch (NotificationNotFoundException ex)
            {
                _logger.LogWarning(ex, "DeleteNotification failed. Notification not found. Id: {id}", notificationId);
                return NotFound();
            }
            catch (NotificationOwnerNotMatchException ex)
            {
                _logger.LogWarning(ex, "DeleteNotification failed. User does not match with notification owner.");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during DeleteNotification.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private static bool IsPaginationValid(int page, int pageSize) => page > 0 && pageSize > 0;

        private Guid? GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || Guid.TryParse(userIdStr, out Guid userId) == false)
                return null;
            return userId;
        }
    }
}
