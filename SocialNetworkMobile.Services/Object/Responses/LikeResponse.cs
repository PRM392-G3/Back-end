namespace SocialNetworkMobile.Services.Object.Responses
{
    public class LikeResponse
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string LikeType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserResponse User { get; set; } = null!;
    }
}
