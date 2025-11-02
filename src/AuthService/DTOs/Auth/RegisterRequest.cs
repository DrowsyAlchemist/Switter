using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "The length of the string should be from 3 to 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Only letters, numbers, and underscores are allowed.")]
        public required string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public required string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*()_+=]).{8,}$",
            ErrorMessage = "The password must have:\r\n" +
            "- at least 8 characters;\r\n- at least one number; \r\n" +
            "- at least one lowercase letter; \r\n" +
            "- at least one capital letter; \r\n" +
            "- at least one special character (for example, @$!%*?&).")]
        public required string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public required string ConfirmPassword { get; set; } = string.Empty;
    }
}
