using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/reservations")]
    [ApiController]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public ReservationController(IReservationService reservationService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _reservationService = reservationService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> CreateReservation(CreateReservationRequestDTO request)
        {
            var auditLog = BuildAuditLog(UserAction.CreateReservation, request.RoomId.ToString());
            var result = await _reservationService.CreateReservationAsync(request, auditLog);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> UpdateReservation(UpdateReservationRequestDTO request)
        {
            var auditLog = BuildAuditLog(UserAction.UpdateReservation, request.Id.ToString());
            var result = await _reservationService.UpdateReservationAsync(request, auditLog);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationResponseDTO>))]
        public async Task<IActionResult> GetReservation(long id)
        {
            var result = await _reservationService.GetReservationByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<ReservationResponseDTO>>))]
        public async Task<IActionResult> GetReservations([FromQuery] GetReservationsInputDTO input)
        {
            var result = await _reservationService.GetReservationsAsync(input);
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationResponseDTO>))]
        public async Task<IActionResult> CancelReservation(long id)
        {
            var auditLog = BuildAuditLog(UserAction.CancelReservation, id.ToString());
            var result = await _reservationService.CancelReservationAsync(id, auditLog);
            return Ok(result);
        }

        [HttpPost("{id}/checkin")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationResponseDTO>))]
        public async Task<IActionResult> CheckIn(long id)
        {
            var auditLog = BuildAuditLog(UserAction.CheckIn, id.ToString());
            var result = await _reservationService.CheckInAsync(id, auditLog);
            return Ok(result);
        }

        [HttpPost("{id}/checkout")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationResponseDTO>))]
        public async Task<IActionResult> CheckOut(long id)
        {
            var auditLog = BuildAuditLog(UserAction.CheckOut, id.ToString());
            var result = await _reservationService.CheckOutAsync(id, auditLog);
            return Ok(result);
        }

        [HttpPut("{id}/override-status")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationResponseDTO>))]
        public async Task<IActionResult> OverrideStatus(long id, [FromBody] string status)
        {
            var auditLog = BuildAuditLog(UserAction.OverrideReservationStatus, id.ToString());
            var result = await _reservationService.OverrideStatusAsync(id, status, auditLog);
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
