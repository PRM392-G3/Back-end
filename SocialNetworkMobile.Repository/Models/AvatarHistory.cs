using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    public class AvatarHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string AvatarUrl { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
