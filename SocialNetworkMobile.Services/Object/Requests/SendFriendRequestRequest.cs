using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class SendFriendRequestRequest
    {
        [Required]
        public int RequesterId { get; set; }

        [Required]
        public int ReceiverId { get; set; }
    }
}

