using TweetService.Infrastructure.Attributes;

namespace TweetService.Infrastructure.Middleware
{
    public class PaginationValidationMiddleware
    {
        private const string PageParameter = "page";
        private const string PageSizeParameter = "pageSize";

        private const int DefaultPage = 1;

        private readonly RequestDelegate _next;
        private readonly ILogger<PaginationValidationMiddleware> _logger;

        public PaginationValidationMiddleware(
            RequestDelegate next,
            ILogger<PaginationValidationMiddleware> logger)
        {
            _next = next;
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
                    pageValue = DefaultPage.ToString();
                if (hasPageSize == false)
                    pageSizeValue = attribute!.DefaultPageSize.ToString();

                if (int.TryParse(pageValue, out int page) == false)
                    page = DefaultPage;
                if (int.TryParse(pageSizeValue, out int pageSize) == false)
                    pageSize = attribute!.DefaultPageSize;

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
