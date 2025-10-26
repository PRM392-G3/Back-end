using System.ComponentModel.DataAnnotations;

namespace SocialNetworkMobile.Services.Object.Requests
{
    public class CreateConversationRequest
    {
        [Required]
        public int User1Id { get; set; }

        [Required]
        public int User2Id { get; set; }
    }
}
