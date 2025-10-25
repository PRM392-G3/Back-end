using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("likes")]
    public class Like
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Column("PostId")]
        public int? PostId { get; set; }

        [Column("CommentId")]
        public int? CommentId { get; set; }

        [Column("ReelId")]
        public int? ReelId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("LikeType")]
        public string LikeType { get; set; } = "LIKE"; // LIKE, LOVE, ANGRY, SAD, etc.

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comment { get; set; }

        [ForeignKey("ReelId")]
        public virtual Reel? Reel { get; set; }
    }
}
