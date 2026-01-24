namespace TweetService.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ValidatePaginationAttribute : Attribute
    {
        public int DefaultPageSize { get; set; } = 20;

        public ValidatePaginationAttribute()
        { }

        public ValidatePaginationAttribute(int defaultPageSize)
        {
            if (defaultPageSize <= 0)
                return;

            DefaultPageSize = defaultPageSize;
        }
    }
}
