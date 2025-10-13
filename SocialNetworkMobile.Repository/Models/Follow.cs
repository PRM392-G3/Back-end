using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("follows")]
    public class Follow
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("FollowerId")]
        public int FollowerId { get; set; }

        [Required]
        [Column("FollowingId")]
        public int FollowingId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual User Follower { get; set; } = null!;

        [ForeignKey("FollowingId")]
        public virtual User Following { get; set; } = null!;
    }
}
