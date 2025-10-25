using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("reels")]
    public class Reel
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [Column("VideoUrl")]
        public string VideoUrl { get; set; } = string.Empty;

        [Column("VideoFileName")]
        public string? VideoFileName { get; set; }

        [MaxLength(300)]
        [Column("Caption")]
        public string? Caption { get; set; }

        // Music fields
        [Column("MusicId")]
        public int? MusicId { get; set; }

        [Column("MusicUrl")]
        public string? MusicUrl { get; set; }

        [Column("MusicFileName")]
        public string? MusicFileName { get; set; }

        [Column("MusicTitle")]
        public string? MusicTitle { get; set; }

        [Column("MusicArtist")]
        public string? MusicArtist { get; set; }

        [Column("MusicDuration")]
        public int MusicDuration { get; set; } = 0;

        [Column("Duration")]
        public int Duration { get; set; } = 0;

        [Column("LikeCount")]
        public int LikeCount { get; set; } = 0;

        [Column("CommentCount")]
        public int CommentCount { get; set; } = 0;

        [Column("ShareCount")]
        public int ShareCount { get; set; } = 0;

        [Column("ViewCount")]
        public int ViewCount { get; set; } = 0;

        [Column("IsPublic")]
        public bool IsPublic { get; set; } = true;

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("MusicId")]
        public virtual ReelMusic? Music { get; set; }
    }
}
