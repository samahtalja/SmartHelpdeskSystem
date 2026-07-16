using System.ComponentModel.DataAnnotations;

namespace SmartHelpdeskSystem.Models
{
    public class Status
    {
        public int StatusId { get; set; }

        [Required]
        public string StatusName { get; set; } = string.Empty;
    }
}