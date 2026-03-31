using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/housekeeping")]
    [ApiController]
    [Authorize]
    public class HousekeepingController : ControllerBase
    {
        private readonly IHousekeepingService _housekeepingService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public HousekeepingController(IHousekeepingService housekeepingService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _housekeepingService = housekeepingService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost("tasks")]
        [Authorize(Roles = "Admin,SuperAdmin,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<HousekeepingTaskResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> CreateTask(CreateHousekeepingTaskDTO request)
        {
            var auditLog = BuildAuditLog(UserAction.CreateHousekeepingTask, request.RoomId.ToString());
            var result = await _housekeepingService.CreateTaskAsync(request, auditLog);
            return Ok(result);
        }

        [HttpGet("tasks/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<HousekeepingTaskResponseDTO>))]
        public async Task<IActionResult> GetTask(long id)
        {
            var result = await _housekeepingService.GetTaskByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("tasks")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<HousekeepingTaskResponseDTO>>))]
        public async Task<IActionResult> GetTasks([FromQuery] GetHousekeepingTasksInputDTO input)
        {
            var result = await _housekeepingService.GetTasksAsync(input);
            return Ok(result);
        }

        [HttpPatch("tasks/{id}/state")]
        [Authorize(Roles = "Admin,SuperAdmin,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<HousekeepingTaskResponseDTO>))]
        public async Task<IActionResult> ChangeTaskState(long id, [FromBody] HousekeepingTaskTrigger trigger)
        {
            var auditLog = BuildAuditLog(UserAction.UpdateHousekeepingTask, id.ToString());
            var result = await _housekeepingService.ChangeTaskStateAsync(id, trigger, auditLog);
            return Ok(result);
        }

        [HttpGet("schedule")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<List<HousekeepingTaskResponseDTO>>))]
        public async Task<IActionResult> GetDailySchedule([FromQuery] long tenantId, [FromQuery] DateTime? date)
        {
            var result = await _housekeepingService.GetDailyScheduleAsync(tenantId, date ?? DateTime.UtcNow);
            return Ok(result);
        }

        private AuditLog BuildAuditLog(string action, string performedAgainst) => new AuditLog
        {
            Action = action,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenHelper.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenHelper.GetUserEmail(Request),
            PerformedAgainst = performedAgainst,
            MacAddress = _tokenHelper.GetMacAddress(Request)
        };
    }
}
