using FeedService.DTOs;
using FeedService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeedService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly Guid DefaultUserId = Guid.Empty;
        private readonly IFeedService _feedService;
        private readonly ILogger<FeedController> _logger;

        public FeedController(IFeedService feedService, ILogger<FeedController> logger)
        {
            _feedService = feedService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedAsync(FeedQuery query)
        {
            if (query.PageSize <= 0)
            {
                _logger.LogWarning("GetFeed failed. Invalid pageSize ({size}).", query.PageSize);
                return BadRequest($"Invalid pageSize ({query.PageSize}).");
            }
            if (query.Cursor < 0)
            {
                _logger.LogWarning("GetFeed failed. Invalid CursorPosition ({position}).", query.Cursor);
                return BadRequest($"Invalid pageSize ({query.PageSize}).");
            }
            try
            {
                var currentUserId = GetCurrentUserId();
                var feedResponse = await _feedService.GetFeedAsync(currentUserId, query);
                _logger.LogInformation("Feed successfully sent. User: {user}", currentUserId);
                return Ok(feedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFeed.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("size")]
        public async Task<IActionResult> GetFeedSizeAsync()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var feedSize = await _feedService.GetFeedSizeAsync(currentUserId);
                _logger.LogInformation("FeedSize successfully sent. User: {user}. Size: {size}", currentUserId, feedSize);
                return Ok(feedSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFeedSize.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpDelete("{tweetId}")]
        public async Task<IActionResult> RemoveFromFeedAsync(Guid tweetId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId == DefaultUserId)
                {
                    _logger.LogWarning("RemoveFromFeed failed. CurrentUser not found.");
                    return Unauthorized();
                }
                await _feedService.RemoveFromFeedAsync(currentUserId, tweetId);
                _logger.LogInformation("Successfully remove tweet from feed. User: {user}. Tweet: {tweet}", currentUserId, tweetId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RemoveFromFeed.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpDelete("deleteUser/{userId}")]
        public async Task<IActionResult> RemoveUserTweetsFromFeedAsync(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId == DefaultUserId)
                {
                    _logger.LogWarning("RemoveFromFeed failed. CurrentUser not found.");
                    return Unauthorized();
                }
                await _feedService.RemoveUserTweetsFromFeedAsync(currentUserId, userId);
                _logger.LogInformation("Successfully remove user tweets from feed.\n" +
                    "CurrentUser: {currentUser}. DeletedUser: {deletedUser}", currentUserId, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RemoveUserTweetsFromFeed.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> RebuildFeedAsync()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId == DefaultUserId)
                {
                    _logger.LogWarning("RebuildFeed failed. CurrentUser not found.");
                    return Unauthorized();
                }
                await _feedService.RebuildFeedAsync(currentUserId);
                _logger.LogInformation("Feed successfully rebuilt. User: {currentUser}", currentUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RebuildFeed.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return DefaultUserId;

            if (Guid.TryParse(userIdClaim, out Guid userId) == false)
                throw new Exception("Invalid UserId format.");

            return userId;
        }
    }
}
