namespace SocialNetworkMobile.Services.Object.Responses
{
    public class FriendshipResponse
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public int ReceiverId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public UserResponse? Requester { get; set; }
        public UserResponse? Receiver { get; set; }
    }
}

