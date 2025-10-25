namespace SocialNetworkMobile.Services.Object.Responses
{
    public class GroupResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public int CreatedById { get; set; }
        public string Privacy { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Optional: Creator info
        public UserResponse? CreatedBy { get; set; }
    }
}

