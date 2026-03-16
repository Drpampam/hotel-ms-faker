using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IPropertyService : IAutoDependencyService
    {
        Task<BaseResponse> AddProperty(AddPropertyRequestDTO request, AuditLog auditLog);
        Task<BaseResponse> UpdateProperty(UpdatePropertyRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<PropertyResponseDTO>> GetById(long id);
        Task<PageBaseResponse<List<PropertyResponseDTO>>> GetTenantPropertyList(GetPropertiesInputDTO input);
    }
}
