using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class FollowRequest
    {
        [Required]
        public Guid FolloweeId { get; set; }
    }
}
