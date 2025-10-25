using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class InviteMemberRequest
    {
        [Required]
        public int GroupId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int InvitedById { get; set; }

        [RegularExpression("^(member|admin)$", ErrorMessage = "Role must be 'member' or 'admin'")]
        public string Role { get; set; } = "member";
    }
}

