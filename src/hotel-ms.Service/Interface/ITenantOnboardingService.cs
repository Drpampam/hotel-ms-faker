using hotelier_core_app.Model.DTOs.Response;

namespace hotelier_core_app.Service.Interface
{
    public interface ITenantOnboardingService : IAutoDependencyService
    {
        Task<BaseResponse<TenantProvisionResponseDTO>> ProvisionTenantAsync(long tenantId, string performedBy);
    }
}
