using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class RespondFriendRequestRequest
    {
        [Required]
        [RegularExpression("^(accepted|rejected)$", ErrorMessage = "Status must be 'accepted' or 'rejected'")]
        public string Status { get; set; } = string.Empty;
    }
}

