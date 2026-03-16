using hotelier_core_app.Core.States;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/rooms")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Controller for managing room operations and state transitions.
    /// </summary>
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomController"/> class.
        /// </summary>
        /// <param name="roomService">Service for room operations.</param>
        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        /// <summary>
        /// Change the state of a room
        /// </summary>
        [HttpPatch("{id}/state")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        /// <summary>
        /// Changes the state of a room.
        /// </summary>
        /// <param name="id">The ID of the room.</param>
        /// <param name="trigger">The trigger to change the room state.</param>
        /// <returns>The result of the state change operation.</returns>
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
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        /// <summary>
        /// Gets the current state of a room.
        /// </summary>
        /// <param name="id">The ID of the room.</param>
        /// <returns>The current state of the room.</returns>
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
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        /// <summary>
        /// Gets available triggers for the current state of a room.
        /// </summary>
        /// <param name="id">The ID of the room.</param>
        /// <returns>The available triggers for the room.</returns>
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
