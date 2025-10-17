using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class UpdateUserRequest
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(1024)]
        public string? AvatarUrl { get; set; }

        [MaxLength(1024)]
        public string? CoverImageUrl { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }
    }
}
