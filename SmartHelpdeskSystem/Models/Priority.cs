using System.ComponentModel.DataAnnotations;

namespace SmartHelpdeskSystem.Models
{
    public class Priority
    {
        public int PriorityId { get; set; }

        [Required]
        public string PriorityName { get; set; } = string.Empty;
    }
}