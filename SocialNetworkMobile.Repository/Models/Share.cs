using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    public class Share
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PostId { get; set; }

        [MaxLength(500)]
        public string? Caption { get; set; }

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (kept for EF Core, even if FKs are not in DB)
        public virtual User User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
    }
}
