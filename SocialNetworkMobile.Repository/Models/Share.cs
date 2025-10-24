using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("shares", Schema = "public")]
    public class Share
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [Column("PostId")]
        public int PostId { get; set; }

        [MaxLength(500)]
        [Column("Caption")]
        public string? Caption { get; set; }

        [Column("IsPublic")]
        public bool IsPublic { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;
    }
}