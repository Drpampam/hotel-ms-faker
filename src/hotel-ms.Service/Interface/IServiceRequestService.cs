using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IServiceRequestService : IAutoDependencyService
    {
        // CRUD
        Task<BaseResponse<ServiceRequestResponseDTO>> CreateServiceRequestAsync(CreateServiceRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<ServiceRequestResponseDTO>> GetServiceRequestByIdAsync(long serviceRequestId);
        Task<PageBaseResponse<List<ServiceRequestResponseDTO>>> GetServiceRequestsAsync(GetServiceRequestsInputDTO input);

        // State machine
        Task<BaseResponse<ServiceRequestStateResponseDTO>> ChangeServiceRequestStateAsync(long serviceRequestId, ServiceRequestTrigger trigger);
        Task<BaseResponse<ServiceRequestStateResponseDTO>> GetServiceRequestStateAsync(long serviceRequestId);
        Task<BaseResponse<List<ServiceRequestTrigger>>> GetAvailableTriggersAsync(long serviceRequestId);
    }
}
