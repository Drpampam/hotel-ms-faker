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
    [Route("api/v1/rooms")]
    [ApiController]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public RoomController(IRoomService roomService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _roomService = roomService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<RoomResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> AddRoom(AddRoomRequestDTO request)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.AddRoom,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = request.Number,
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            var result = await _roomService.AddRoomAsync(request, auditLog);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<RoomResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> UpdateRoom(UpdateRoomRequestDTO request)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.UpdateRoom,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = request.Id.ToString(),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            var result = await _roomService.UpdateRoomAsync(request, auditLog);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<RoomResponseDTO>))]
        public async Task<IActionResult> GetRoom(long id)
        {
            var result = await _roomService.GetRoomByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<RoomResponseDTO>>))]
        public async Task<IActionResult> GetRooms([FromQuery] GetRoomsInputDTO input)
        {
            var result = await _roomService.GetRoomsAsync(input);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteRoom(long id)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.DeleteRoom,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = id.ToString(),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            var result = await _roomService.DeleteRoomAsync(id, auditLog);
            return Ok(result);
        }

        /// <summary>
        /// Change the state of a room
        /// </summary>
        [HttpPatch("{id}/state")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeRoomState(long id, [FromBody] RoomTrigger trigger)
        {
            var result = await _roomService.ChangeRoomStateAsync(id, trigger);
            if (result.Status)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Get the current state of a room
        /// </summary>
        [HttpGet("{id}/state")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRoomState(long id)
        {
            var state = await _roomService.GetRoomStateAsync(id);
            if (state == null)
                return NotFound();
            if (!state.Status && state.StatusCode == ResponseStatusCode.NoRecordFound)
                return NotFound(state);
            if (!state.Status)
                return BadRequest(state);
            return Ok(state);
        }

        /// <summary>
        /// Get available triggers for the current state of a room
        /// </summary>
        [HttpGet("{id}/triggers")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Housekeeping,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAvailableRoomTriggers(long id)
        {
            var triggers = await _roomService.GetAvailableTriggersAsync(id);
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
