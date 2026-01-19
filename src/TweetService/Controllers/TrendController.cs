using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TweetService.Interfaces.Services;

namespace TweetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendController : ControllerBase
    {
        private readonly ITrendService _trendService;
        private readonly ILogger<TrendController> _logger;

        public TrendController(ITrendService trendService, ILogger<TrendController> logger)
        {
            _trendService = trendService;
            _logger = logger;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetTrendCategoriesAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (ValidatePagination(page, pageSize) == false)
            {
                _logger.LogWarning("GetTrendCategoriesAsync failed.\nPagination is incorrect.\nPage: {page}\nSize: {pageSize}", page, pageSize);
                return BadRequest("Pagination is incorrect.");
            }
            try
            {
                var trendCategories = await _trendService.GetTrendCategoriesAsync(page, pageSize);
                _logger.LogInformation("Trend categories successfully sent.");
                return Ok(trendCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTrendCategories.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("tweets")]
        public async Task<IActionResult> GetTrendTweetsAsync(int page = 1, int pageSize = 20)
        {
            if (ValidatePagination(page, pageSize) == false)
            {
                _logger.LogWarning("GetTrendTweetsAsync failed.\nPagination is incorrect.\nPage: {page}\nSize: {pageSize}", page, pageSize);
                return BadRequest("Pagination is incorrect.");
            }
            try
            {
                var currentUserId = GetCurrentUserId();
                var tweetDto = await _trendService.GetTrendTweetsAsync(currentUserId, page, pageSize);
                _logger.LogInformation("Trend tweets successfully sent.");
                return Ok(tweetDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTrendTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("tweets/{hashtag}")]
        public async Task<IActionResult> GetTrendTweetsAsync(string hashtag, int page = 1, int pageSize = 20)
        {
            if (ValidatePagination(page, pageSize) == false)
            {
                _logger.LogWarning("GetTrendTweetsAsync failed.\nPagination is incorrect.\nPage: {page}\nSize: {pageSize}", page, pageSize);
                return BadRequest("Pagination is incorrect.");
            }
            try
            {
                if (string.IsNullOrEmpty(hashtag))
                    return BadRequest("Hashtag shouldn't be null or empty.");

                var currentUserId = GetCurrentUserId();
                var tweetDto = await _trendService.GetTrendTweetsAsync(hashtag, currentUserId, page, pageSize);
                _logger.LogInformation("Trend tweets by hashtag successfully sent.\nHashtag: {hashtag}", hashtag);
                return Ok(tweetDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTrendTweets.\nHashtag: {hashtag}", hashtag);
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

        private bool ValidatePagination(int page, int pageSize) => (page > 0 && pageSize > 0);
    }
}