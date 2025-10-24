using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class SharePostRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int PostId { get; set; }

        [MaxLength(500)]
        public string? Caption { get; set; }

        public bool IsPublic { get; set; } = true;
    }
}