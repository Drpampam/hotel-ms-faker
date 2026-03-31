using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IHousekeepingService : IAutoDependencyService
    {
        Task<BaseResponse<HousekeepingTaskResponseDTO>> CreateTaskAsync(CreateHousekeepingTaskDTO request, AuditLog auditLog);
        Task<BaseResponse<HousekeepingTaskResponseDTO>> GetTaskByIdAsync(long taskId);
        Task<PageBaseResponse<List<HousekeepingTaskResponseDTO>>> GetTasksAsync(GetHousekeepingTasksInputDTO input);
        Task<BaseResponse<HousekeepingTaskResponseDTO>> ChangeTaskStateAsync(long taskId, HousekeepingTaskTrigger trigger, AuditLog auditLog);
        Task<BaseResponse<List<HousekeepingTaskResponseDTO>>> GetDailyScheduleAsync(long tenantId, DateTime date);
    }
}
