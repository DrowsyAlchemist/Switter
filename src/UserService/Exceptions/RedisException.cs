namespace UserService.Exceptions
{
    public class RedisException : Exception
    {
        public RedisException(string? message) : base(message) { }
        public RedisException(string? message, Exception? innerEx) : base(message, innerEx) { }
    }
}
