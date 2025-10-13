using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("post_tags")]
    public class PostTag
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("PostId")]
        public int PostId { get; set; }

        [Required]
        [Column("TagId")]
        public int TagId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }
}
