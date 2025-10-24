namespace SocialNetworkMobile.Services.Object.Responses
{
    public class GroupMemberResponse
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public int? InvitedById { get; set; }
        
        // User info
        public UserResponse? User { get; set; }
        
        // Group info (optional)
        public GroupResponse? Group { get; set; }
    }
}

