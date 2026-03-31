using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class HousekeepingTaskResponseDTO
    {
        public long Id { get; set; }
        public long RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public long? AssignedToUserId { get; set; }
        public string? AssignedToName { get; set; }
        public string? TaskType { get; set; }
        public string? Priority { get; set; }
        public string? Notes { get; set; }
        public HousekeepingTaskState State { get; set; }
        public List<HousekeepingTaskTrigger>? AvailableTriggers { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? TenantId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
