namespace SocialNetworkMobile.Services.Object.Responses
{
    public class GroupChatMessageResponse
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatarUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
