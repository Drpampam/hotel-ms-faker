using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface;

public interface IActivationService : IAutoDependencyService
{
    Task<BaseResponse<ActivationCodeResponseDTO>> GenerateActivationCodeAsync(GenerateActivationCodeRequestDTO request, AuditLog auditLog);
    Task<BaseResponse<ActivateTenantResponseDTO>> ActivateTenantAsync(ActivateTenantRequestDTO request, string ipAddress);
    Task<BaseResponse<SubscriptionStatusResponseDTO>> GetSubscriptionStatusAsync(long tenantId);
    Task<BaseResponse> RenewSubscriptionAsync(long tenantId, string code, string callerEmail, AuditLog auditLog);
}
