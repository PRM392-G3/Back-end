using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class CreateNotificationRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int FromUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        public int? PostId { get; set; }

        public int? CommentId { get; set; }
    }
}
