using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("comments")]
    public class Comment
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("PostId")]
        public int PostId { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        [Column("Content")]
        public string Content { get; set; } = string.Empty;

        [Column("ParentCommentId")]
        public int? ParentCommentId { get; set; }

        [Column("LikeCount")]
        public int LikeCount { get; set; } = 0;

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
