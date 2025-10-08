namespace AuthService.DTOs.Jwt
{
    public class ValidateTokenResult
    {
        public Guid? UserId { get; set; }
        public bool Success {  get; set; }
        public Exception? Exception { get; set; }
    }
}
