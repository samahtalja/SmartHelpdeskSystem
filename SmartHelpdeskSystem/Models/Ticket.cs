using System.ComponentModel.DataAnnotations;

namespace SmartHelpdeskSystem.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
       

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DueDate { get; set; }
        public string? UserId { get; set; }

        public string? AssignedAgentId { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int PriorityId { get; set; }
        public Priority? Priority { get; set; }

        public int StatusId { get; set; }
        public Status? Status { get; set; }
    }
}