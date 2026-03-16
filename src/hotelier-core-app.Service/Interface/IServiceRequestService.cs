using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Response;

namespace hotelier_core_app.Service.Interface
{
    public interface IServiceRequestService : IAutoDependencyService
    {
        Task<BaseResponse<ServiceRequestStateResponseDTO>> ChangeServiceRequestStateAsync(long serviceRequestId, ServiceRequestTrigger trigger);
        Task<BaseResponse<ServiceRequestStateResponseDTO>> GetServiceRequestStateAsync(long serviceRequestId);
        Task<BaseResponse<List<ServiceRequestTrigger>>> GetAvailableTriggersAsync(long serviceRequestId);
    }
}
