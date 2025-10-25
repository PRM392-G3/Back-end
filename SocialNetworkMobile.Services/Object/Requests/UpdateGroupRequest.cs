using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class UpdateGroupRequest
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? AvatarUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        [RegularExpression("^(public|private)$", ErrorMessage = "Privacy must be 'public' or 'private'")]
        public string? Privacy { get; set; }
    }
}

