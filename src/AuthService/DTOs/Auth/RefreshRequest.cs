using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs.Auth
{
    public class RefreshRequest
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
