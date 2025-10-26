namespace SocialNetworkMobile.Services.Object.Responses
{
    public class ConversationResponse
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        public string User1Name { get; set; } = string.Empty;
        public string User2Name { get; set; } = string.Empty;
        public string User1AvatarUrl { get; set; } = string.Empty;
        public string User2AvatarUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public MessageResponse? LastMessage { get; set; }
    }
}
