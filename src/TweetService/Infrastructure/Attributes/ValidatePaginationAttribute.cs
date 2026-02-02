namespace TweetService.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ValidatePaginationAttribute : Attribute
    {
    }
}
