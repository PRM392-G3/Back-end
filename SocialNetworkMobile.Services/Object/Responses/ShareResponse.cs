namespace SocialNetworkMobile.Services.Object.Responses
{
    public class ShareResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public string? Caption { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserResponse? User { get; set; }
        public PostResponse? Post { get; set; }
    }
}