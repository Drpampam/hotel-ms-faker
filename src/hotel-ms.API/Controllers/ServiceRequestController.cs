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
    [Route("api/v1/service-requests")]
    [ApiController]
    [Authorize]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public ServiceRequestController(IServiceRequestService serviceRequestService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _serviceRequestService = serviceRequestService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Guest,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ServiceRequestResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> CreateServiceRequest(CreateServiceRequestDTO request)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.CreateServiceRequest,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = request.ReservationId.ToString(),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            var result = await _serviceRequestService.CreateServiceRequestAsync(request, auditLog);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Guest,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ServiceRequestResponseDTO>))]
        public async Task<IActionResult> GetServiceRequest(long id)
        {
            var roles = _tokenHelper.GetUserRoles(Request);
            var callerEmail = IsGuestOnly(roles) ? _tokenHelper.GetUserEmail(Request) : null;
            var result = await _serviceRequestService.GetServiceRequestByIdAsync(id, callerEmail);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Guest,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<ServiceRequestResponseDTO>>))]
        public async Task<IActionResult> GetServiceRequests([FromQuery] GetServiceRequestsInputDTO input)
        {
            var roles = _tokenHelper.GetUserRoles(Request);
            var callerEmail = IsGuestOnly(roles) ? _tokenHelper.GetUserEmail(Request) : null;
            var result = await _serviceRequestService.GetServiceRequestsAsync(input, callerEmail);
            return Ok(result);
        }

        private static bool IsGuestOnly(List<string> roles)
        {
            var staffRoles = new[] { "SuperAdmin", "Admin", "FrontDesk", "Housekeeping", "Developer" };
            return roles.Contains("Guest") && !staffRoles.Any(roles.Contains);
        }

        /// <summary>
        /// Change the state of a service request
        /// </summary>
        [HttpPatch("{id}/state")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeServiceRequestState(long id, [FromBody] ServiceRequestTrigger trigger)
        {
            var result = await _serviceRequestService.ChangeServiceRequestStateAsync(id, trigger);
            if (result.Status)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Get the current state of a service request
        /// </summary>
        [HttpGet("{id}/state")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetServiceRequestState(long id)
        {
            var state = await _serviceRequestService.GetServiceRequestStateAsync(id);
            if (state == null)
                return NotFound();
            if (!state.Status && state.StatusCode == ResponseStatusCode.NoRecordFound)
                return NotFound(state);
            if (!state.Status)
                return BadRequest(state);
            return Ok(state);
        }

        /// <summary>
        /// Get available triggers for the current state of a service request
        /// </summary>
        [HttpGet("{id}/triggers")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAvailableServiceRequestTriggers(long id)
        {
            var triggers = await _serviceRequestService.GetAvailableTriggersAsync(id);
            if (triggers == null)
                return NotFound();
            if (!triggers.Status && triggers.StatusCode == ResponseStatusCode.NoRecordFound)
                return NotFound(triggers);
            if (!triggers.Status)
                return BadRequest(triggers);
            return Ok(triggers);
        }
    }
}
