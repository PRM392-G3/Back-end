using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class SendGroupMessageRequest
    {
        [Required]
        public int GroupId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
