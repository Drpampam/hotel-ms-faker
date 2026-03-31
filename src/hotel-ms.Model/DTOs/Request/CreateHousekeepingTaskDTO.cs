using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class CreateHousekeepingTaskDTO
    {
        [Required]
        public long RoomId { get; set; }

        public long? AssignedToUserId { get; set; }

        [Required]
        [StringLength(100)]
        public string TaskType { get; set; } = "Cleaning";

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public long? TenantId { get; set; }
    }
}
