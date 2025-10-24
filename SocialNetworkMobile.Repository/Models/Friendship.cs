using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("friendships")]
    public class Friendship
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("RequesterId")]
        public int RequesterId { get; set; }

        [Required]
        [Column("ReceiverId")]
        public int ReceiverId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "pending"; // pending, accepted, rejected, blocked

        [Column("RequestedAt")]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Column("RespondedAt")]
        public DateTime? RespondedAt { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("RequesterId")]
        public virtual User Requester { get; set; } = null!;

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; } = null!;
    }

    // Enum for Friendship Status
    public static class FriendshipStatus
    {
        public const string Pending = "pending";
        public const string Accepted = "accepted";
        public const string Rejected = "rejected";
        public const string Blocked = "blocked";
    }
}

