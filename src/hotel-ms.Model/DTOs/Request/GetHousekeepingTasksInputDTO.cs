using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetHousekeepingTasksInputDTO
    {
        public long? RoomId { get; set; }
        public long? AssignedToUserId { get; set; }
        public string? TaskType { get; set; }
        public HousekeepingTaskState? State { get; set; }
        public long? TenantId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
