using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [Column("FromUserId")]
        public int FromUserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Type")]
        public string Type { get; set; } = string.Empty; // LIKE, COMMENT, FOLLOW, MENTION

        [MaxLength(500)]
        [Column("Title")]
        public string? Title { get; set; }

        [MaxLength(1000)]
        [Column("Message")]
        public string? Message { get; set; }

        [Column("PostId")]
        public int? PostId { get; set; }

        [Column("CommentId")]
        public int? CommentId { get; set; }

        [Column("IsRead")]
        public bool IsRead { get; set; } = false;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("FromUserId")]
        public virtual User FromUser { get; set; } = null!;
    }
}
