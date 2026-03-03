using Microsoft.Extensions.Options;
using TweetService.Infrastructure.Attributes;
using TweetService.Models.Options;

namespace TweetService.Infrastructure.Middleware
{
    public class PaginationValidationMiddleware
    {
        private const string PageParameter = "page";
        private const string PageSizeParameter = "pageSize";

        private readonly RequestDelegate _next;
        private readonly PaginationOptions _options;
        private readonly ILogger<PaginationValidationMiddleware> _logger;

        public PaginationValidationMiddleware(
            RequestDelegate next,
            IOptions<PaginationOptions> options,
            ILogger<PaginationValidationMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var attribute = endpoint?.Metadata.GetMetadata<ValidatePaginationAttribute>();

            if (attribute != null)
            {
                bool hasPage = context.Request.Query.TryGetValue(PageParameter, out var pageValue);
                bool hasPageSize = context.Request.Query.TryGetValue(PageSizeParameter, out var pageSizeValue);

                if (hasPage == false)
                    pageValue = _options.DefaultPage.ToString();
                if (hasPageSize == false)
                    pageSizeValue = _options.DefaultPageSize.ToString();

                if (int.TryParse(pageValue, out int page) == false)
                    page = _options.DefaultPage;
                if (int.TryParse(pageSizeValue, out int pageSize) == false)
                    pageSize = _options.DefaultPageSize;

                if (page <= 0 || pageSize <= 0)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Pagination parameters must be positive numbers",
                        page,
                        pageSize
                    });
                    _logger.LogWarning("Pagination parameters is invalid.\n Page: {page}.\n PageSize: {pageSize}.\n Endpoint: {endpoint}",
                        pageValue, pageSizeValue, endpoint?.DisplayName);
                    return;
                }
            }
            await _next(context);
        }
    }
}
