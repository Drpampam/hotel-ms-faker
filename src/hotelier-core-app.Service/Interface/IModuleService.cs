using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IModuleService : IAutoDependencyService
    {
        Task<BaseResponse> CreateModuleGroup(CreateModuleGroupDTO model, AuditLog auditLog);

        Task<BaseResponse> EditModuleGroup(long id, EditModuleGroupDTO model, AuditLog auditLog);

        Task<BaseResponse> DeleteModuleGroup(long id, AuditLog auditLog);

        Task<BaseResponse<List<ModuleGroupDTO>>> GetAllModuleGroup();

        Task<BaseResponse> CreateModule(CreateModuleDTO model, AuditLog auditLog);

        Task<BaseResponse> EditModule(long id, EditModuleDTO model, AuditLog auditLog);

        Task<BaseResponse> DeleteModule(long id, AuditLog auditLog);

        Task<BaseResponse<List<ModuleDTO>>> GetAllModule();

        BaseResponse<List<ModuleGroupDTO>> GetAssignedModules(List<string> roles);
    }
}
