using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class SendMessageRequest
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        public int SenderId { get; set; }

        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        public string? VideoUrl { get; set; }
    }
}
