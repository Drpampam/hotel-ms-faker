using hotelier_core_app.Core.States;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/service-requests")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Controller for managing service request operations and state transitions.
    /// </summary>
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRequestController"/> class.
        /// </summary>
        /// <param name="serviceRequestService">Service for service request operations.</param>
        public ServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        /// <summary>
        /// Change the state of a service request
        /// </summary>
        [HttpPatch("{id}/state")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        /// <summary>
        /// Changes the state of a service request.
        /// </summary>
        /// <param name="id">The ID of the service request.</param>
        /// <param name="trigger">The trigger to change the service request state.</param>
        /// <returns>The result of the state change operation.</returns>
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
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        /// <summary>
        /// Gets the current state of a service request.
        /// </summary>
        /// <param name="id">The ID of the service request.</param>
        /// <returns>The current state of the service request.</returns>
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
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        /// <summary>
        /// Gets available triggers for the current state of a service request.
        /// </summary>
        /// <param name="id">The ID of the service request.</param>
        /// <returns>The available triggers for the service request.</returns>
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
