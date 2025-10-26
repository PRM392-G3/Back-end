using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class CreatePostRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        public List<string>? Tags { get; set; }

        public int? GroupId { get; set; }
    }
}
