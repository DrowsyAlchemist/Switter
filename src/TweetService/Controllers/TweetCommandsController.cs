using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Infrastructure.Attributes;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;

namespace TweetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TweetCommandsController : ControllerBase
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly IUserServiceClient _userServiceClient;
        private readonly ILogger<TweetQueriesController> _logger;

        public TweetCommandsController(
            ITweetCommands tweetCommands,
            IUserServiceClient userServiceClient,
            ILogger<TweetQueriesController> logger)
        {
            _tweetCommands = tweetCommands;
            _userServiceClient = userServiceClient;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> TweetAsync(CreateTweetRequest request, [CurrentUserId] Guid? currentUserId)
        {
            try
            {
                if (currentUserId.HasValue == false)
                    throw new Exception("Current user not found.");

                var userInfo = await _userServiceClient.GetUserInfoAsync(currentUserId.Value);
                if (userInfo == null)
                    throw new UserNotFoundException(currentUserId.Value);

                await _tweetCommands.TweetAsync(userInfo, request);
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
        public async Task<IActionResult> DeleteTweetAsync(Guid tweetId, [CurrentUserId] Guid? currentUserId)
        {
            try
            {
                if (currentUserId.HasValue == false)
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
    }
}
