namespace SocialNetworkMobile.Services.Object.Responses
{
    public class ReelMusicResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Artist { get; set; }
        public string MusicUrl { get; set; } = string.Empty;
        public int? Duration { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
