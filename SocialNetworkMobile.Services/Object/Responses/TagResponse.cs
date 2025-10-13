namespace SocialNetworkMobile.Services.Object.Responses
{
    public class TagResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
