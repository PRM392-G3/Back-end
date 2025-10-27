using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class UpdateFcmTokenRequest
    {
        [Required]
        public string FcmToken { get; set; } = string.Empty;
    }
}

