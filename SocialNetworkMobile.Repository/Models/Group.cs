using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("groups")]
    public class Group
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("Description")]
        public string? Description { get; set; }

        [MaxLength(1024)]
        [Column("AvatarUrl")]
        public string? AvatarUrl { get; set; }

        [MaxLength(1024)]
        [Column("CoverImageUrl")]
        public string? CoverImageUrl { get; set; }

        [Required]
        [Column("CreatedById")]
        public int CreatedById { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("Privacy")]
        public string Privacy { get; set; } = "public"; // public, private

        [Column("MemberCount")]
        public int MemberCount { get; set; } = 0;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CreatedById")]
        public virtual User CreatedBy { get; set; } = null!;

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    }

    // Group Privacy Constants
    public static class GroupPrivacy
    {
        public const string Public = "public";
        public const string Private = "private";
    }
}

