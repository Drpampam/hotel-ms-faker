using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/audit-logs")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin,Developer")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<AuditLogResponseDTO>>))]
        public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsInputDTO input)
        {
            var result = await _auditLogService.GetAuditLogsAsync(input);
            return Ok(result);
        }
    }
}
