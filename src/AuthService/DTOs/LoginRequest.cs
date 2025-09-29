using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string IpAddress { get; set; } = "Unknown";
    }
}
