using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class CreateCommentRequest
    {
        public int? PostId { get; set; }

        public int? ReelId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
    }
}
