using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("group_members")]
    public class GroupMember
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("GroupId")]
        public int GroupId { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("Role")]
        public string Role { get; set; } = "member"; // admin, moderator, member

        [Required]
        [MaxLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "active"; // pending, active, banned

        [Column("JoinedAt")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [Column("InvitedById")]
        public int? InvitedById { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("InvitedById")]
        public virtual User? InvitedBy { get; set; }
    }

    // Group Member Role Constants
    public static class GroupMemberRole
    {
        public const string Admin = "admin";
        public const string Moderator = "moderator";
        public const string Member = "member";
    }

    // Group Member Status Constants
    public static class GroupMemberStatus
    {
        public const string Pending = "pending";
        public const string Active = "active";
        public const string Banned = "banned";
    }
}

