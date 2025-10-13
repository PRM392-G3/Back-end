using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("user_social_providers")]
    public class UserSocialProvider
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("ProviderName")]
        public string ProviderName { get; set; } = string.Empty; // GOOGLE, FACEBOOK, etc.

        [Required]
        [MaxLength(255)]
        [Column("ProviderId")]
        public string ProviderId { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("Email")]
        public string? Email { get; set; }

        [MaxLength(2048)]
        [Column("ProfileUrl")]
        public string? ProfileUrl { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
