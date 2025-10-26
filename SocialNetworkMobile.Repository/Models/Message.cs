using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkMobile.Repository.Models
{
    [Table("messages")]
    public class Message
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("conversationid")]
        public int ConversationId { get; set; }

        [Required]
        [Column("senderid")]
        public int SenderId { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [MaxLength(500)]
        [Column("imageurl")]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        [Column("videourl")]
        public string? VideoUrl { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;
    }
}
