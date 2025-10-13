using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class UpdatePostRequest
    {
        [MaxLength(2000)]
        public string? Content { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        public List<string>? Tags { get; set; }
    }
}
