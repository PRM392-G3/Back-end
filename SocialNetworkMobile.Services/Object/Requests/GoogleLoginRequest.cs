using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class GoogleLoginRequest
    {
        [Required]
        public string GoogleToken { get; set; } = string.Empty;
    }
}
