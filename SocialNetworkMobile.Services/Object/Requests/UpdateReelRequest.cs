namespace SocialNetworkMobile.Services.Object.Requests
{
    public class UpdateReelRequest
    {
        public string? Caption { get; set; }
        public bool? IsPublic { get; set; }
        public string? VideoUrl { get; set; }
        public string? VideoFileName { get; set; }
        public int? MusicId { get; set; }
        public string? MusicUrl { get; set; }
        public string? MusicFileName { get; set; }
        public string? MusicTitle { get; set; }
        public string? MusicArtist { get; set; }
        public int? MusicDuration { get; set; }
        public int? Duration { get; set; }
    }
}
