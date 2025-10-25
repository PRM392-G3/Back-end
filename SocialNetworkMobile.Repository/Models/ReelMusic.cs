using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("reel_music")]
    public class ReelMusic
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("Title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("Artist")]
        public string? Artist { get; set; }

        [Required]
        [Column("MusicUrl")]
        public string MusicUrl { get; set; } = string.Empty;

        [Column("Duration")]
        public int? Duration { get; set; }

        [Column("CoverImageUrl")]
        public string? CoverImageUrl { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
