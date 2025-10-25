using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class CreateReelMusicRequest
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Artist { get; set; }

        [Required]
        public string MusicUrl { get; set; } = string.Empty;

        public int? Duration { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}
