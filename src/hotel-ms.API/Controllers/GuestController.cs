using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/guests")]
    [ApiController]
    [Authorize]
    public class GuestController : ControllerBase
    {
        private readonly IGuestService _guestService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public GuestController(IGuestService guestService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _guestService = guestService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<GuestProfileResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> CreateGuestProfile(CreateGuestProfileRequestDTO request)
        {
            var auditLog = BuildAuditLog(UserAction.CreateGuestProfile, request.UserId.ToString());
            var result = await _guestService.CreateGuestProfileAsync(request, auditLog);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<GuestProfileResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> UpdateGuestProfile(UpdateGuestProfileRequestDTO request)
        {
            var auditLog = BuildAuditLog(UserAction.UpdateGuestProfile, request.Id.ToString());
            var result = await _guestService.UpdateGuestProfileAsync(request, auditLog);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<GuestProfileResponseDTO>))]
        public async Task<IActionResult> GetGuestProfile(long id)
        {
            var result = await _guestService.GetGuestProfileByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("by-user/{userId}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<GuestProfileResponseDTO>))]
        public async Task<IActionResult> GetGuestProfileByUser(long userId)
        {
            var result = await _guestService.GetGuestProfileByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<GuestProfileResponseDTO>>))]
        public async Task<IActionResult> GetGuests([FromQuery] GetGuestsInputDTO input)
        {
            var result = await _guestService.GetGuestsAsync(input);
            return Ok(result);
        }

        [HttpGet("{id}/reservations")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<ReservationResponseDTO>>))]
        public async Task<IActionResult> GetGuestReservations(long id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _guestService.GetGuestReservationsAsync(id, pageNumber, pageSize);
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
