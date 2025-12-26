using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Services;

namespace TweetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TweetController : ControllerBase
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly ITweetQueries _tweetQueries;
        private readonly ILogger<TweetController> _logger;

        public TweetController(ITweetCommands tweetCommands, ITweetQueries tweetQueries, ILogger<TweetController> logger)
        {
            _tweetCommands = tweetCommands;
            _tweetQueries = tweetQueries;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> TweetAsync(CreateTweetRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                    throw new Exception("Current user not found.");

                await _tweetCommands.TweetAsync(currentUserId.Value, request);
                _logger.LogInformation("Successfully tweet.");
                return Ok(new { message = "Successfully tweet" });
            }
            catch (ParentTweetNullException ex)
            {
                _logger.LogWarning(ex, "Tweet failed.\nParent tweet is null.");
                return BadRequest("User not found.");
            }
            catch (SelfRetweetException ex)
            {
                _logger.LogWarning(ex, "Tweet failed.\nSelf retweet attempt.");
                return BadRequest("Can't retweet yourself.");
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tweet author not found.");
                return StatusCode(500, new { message = "Internal server error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Tweet.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpDelete("{tweetId}")]
        public async Task<IActionResult> DeleteTweetAsync(Guid tweetId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                    throw new Exception("Current user not found.");

                await _tweetCommands.DeleteTweetAsync(tweetId, currentUserId.Value);
                _logger.LogInformation("Tweet successfully deleted.\nTweetId: {tweetId}", tweetId);
                return Ok(new { message = "Tweet successfully deleted." });
            }
            catch (TweetNotFoundException ex)
            {
                _logger.LogWarning(ex, "DeleteTweet failed.\nTweet is not found.");
                return NotFound("Tweet not found.");
            }
            catch (DeleteTweetForbiddenException ex)
            {
                _logger.LogWarning(ex, "DeleteTweet failed.\nTweet can be deleted only by its author.");
                return Forbid("Action is forbidden.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during DeleteTweet.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{tweetId}")]
        public async Task<IActionResult> GetTweetAsync(Guid tweetId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var tweetDto = await _tweetQueries.GetTweetAsync(tweetId, currentUserId);
                _logger.LogInformation("Tweet successfully sent.\nId:{tweetId}", tweetId);
                return Ok(tweetDto);
            }
            catch (TweetNotFoundException ex)
            {
                _logger.LogWarning("GetTweet failed. Tweet not found.\nId: {tweetId}", tweetId);
                return NotFound("Tweet not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTweet.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTweetsAsync(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var tweetDtos = await _tweetQueries.GetUserTweetsAsync(userId, page, pageSize, currentUserId);
                _logger.LogInformation("User's tweets successfully sent.\nUserId:{userId}", userId);
                return Ok(tweetDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetUserTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyTweets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                    throw new Exception("Current user not found.");

                var tweetDtos = await _tweetQueries.GetUserTweetsAsync(currentUserId.Value, page, pageSize, currentUserId);
                _logger.LogInformation("User's tweets successfully sent.\nUserId:{userId}", currentUserId.Value);
                return Ok(tweetDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetMyTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        [HttpGet("replies/{tweetId}")]
        public async Task<IActionResult> GetTweetRepliesAsync(Guid tweetId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var tweetDtos = await _tweetQueries.GetTweetRepliesAsync(tweetId, page, pageSize, currentUserId);
                _logger.LogInformation("Tweet replies successfully sent.\nTweetId:{tweetId}", tweetId);
                return Ok(tweetDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTweetReplies.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
                return Guid.Parse(userId!);

            return null;
        }
    }
}
