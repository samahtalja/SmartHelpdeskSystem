using System.ComponentModel.DataAnnotations;

namespace SmartHelpdeskSystem.Models
{
    public class TicketComment
    {
        public int TicketCommentId { get; set; }

        [Required]
        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}