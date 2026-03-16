using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface;

public interface ISubscriptionService : IAutoDependencyService
{
    Task<BaseResponse> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDTO request, AuditLog auditLog);
    Task<BaseResponse<SubscriptionPlanResponseDTO>> GetSubscriptionPlanByIdAsync(long id);
    Task<BaseResponse<List<SubscriptionPlanResponseDTO>>> GetAllSubscriptionPlansAsync();
    Task<BaseResponse> AssignSubscriptionPlanToTenantAsync(AssignSubscriptionPlanDTO request, AuditLog auditLog);
    Task<BaseResponse> DeleteSubscriptionPlanAsync(long id, AuditLog auditLog);
}