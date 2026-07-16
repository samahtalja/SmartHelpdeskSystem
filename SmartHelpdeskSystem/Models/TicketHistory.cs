using System.ComponentModel.DataAnnotations;

namespace SmartHelpdeskSystem.Models
{
    public class TicketHistory
    {
        public int TicketHistoryId { get; set; }

        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public string ChangedByUserId { get; set; } = string.Empty;

        public int? OldStatusId { get; set; }
        public Status? OldStatus { get; set; }

        public int? NewStatusId { get; set; }
        public Status? NewStatus { get; set; }

        public string? ChangeNote { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}