using System.ComponentModel.DataAnnotations;

namespace SmartHelpdeskSystem.Models
{
    public class Attachment
    {
        public int AttachmentId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public int TicketId { get; set; }

        public Ticket? Ticket { get; set; }
    }
}