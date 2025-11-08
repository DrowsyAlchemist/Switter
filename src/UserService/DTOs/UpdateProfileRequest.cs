using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UpdateProfileRequest
    {
        [MinLength(3)]
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [Url]
        public string? AvatarUrl { get; set; }
    }
}
