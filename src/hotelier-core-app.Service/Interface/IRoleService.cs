using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IRoleService : IAutoDependencyService
    {
        Task<BaseResponse> CreateRoleAsync(CreateRoleRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<RoleResponseDTO>> GetRoleByIdAsync(long roleId);
        Task<BaseResponse<List<RoleResponseDTO>>> GetAllRolesAsync();
        Task<BaseResponse> UpdateRoleAsync(UpdateRoleRequestDTO request, AuditLog auditLog);
        Task<BaseResponse> DeleteRoleAsync(long roleId, AuditLog auditLog);
    }
}