using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("PasswordHash")]
        public string? PasswordHash { get; set; }

        [MaxLength(255)]
        [Column("FullName")]
        public string? FullName { get; set; }

        [MaxLength(1024)]
        [Column("AvatarUrl")]
        public string? AvatarUrl { get; set; }

        [MaxLength(20)]
        [Column("PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        [Column("Bio")]
        public string? Bio { get; set; }

        [Column("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(100)]
        [Column("Location")]
        public string? Location { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("EmailVerifiedAt")]
        public DateTime? EmailVerifiedAt { get; set; }

        [Column("LastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();
        public virtual ICollection<UserSocialProvider> UserSocialProviders { get; set; } = new List<UserSocialProvider>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
        public virtual ICollection<Share> Shares { get; set; } = new List<Share>();
    }
}
