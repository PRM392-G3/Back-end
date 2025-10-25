using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class CreateReelRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string VideoUrl { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Caption { get; set; }

        public int? MusicId { get; set; }

        public bool IsPublic { get; set; } = true;
    }
}
