using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;

namespace hotelier_core_app.Service.Interface
{
    public interface IAuditLogService : IAutoDependencyService
    {
        Task<PageBaseResponse<List<AuditLogResponseDTO>>> GetAuditLogsAsync(GetAuditLogsInputDTO input);
    }
}
