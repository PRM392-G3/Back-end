using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(2000)]
        [Column("Content")]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("ImageUrl")]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        [Column("VideoUrl")]
        public string? VideoUrl { get; set; }

        [Column("LikeCount")]
        public int LikeCount { get; set; } = 0;

        [Column("CommentCount")]
        public int CommentCount { get; set; } = 0;

        [Column("ShareCount")]
        public int ShareCount { get; set; } = 0;

        [Column("IsPublic")]
        public bool IsPublic { get; set; } = true;

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("GroupId")]
        public int? GroupId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }
        
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public virtual ICollection<Share> Shares { get; set; } = new List<Share>();
    }
}
