namespace SocialNetworkMobile.Services.Object.Responses
{
    public class NotificationResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FromUserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Message { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserResponse FromUser { get; set; } = null!;
    }
}
