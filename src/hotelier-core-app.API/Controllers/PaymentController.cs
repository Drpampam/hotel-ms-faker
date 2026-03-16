using hotelier_core_app.Core.States;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/payments")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Controller for managing payment operations and state transitions.
    /// </summary>
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentController"/> class.
        /// </summary>
        /// <param name="paymentService">Service for payment operations.</param>
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Change the state of a payment
        /// </summary>
        [HttpPatch("{id}/state")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        /// <summary>
        /// Changes the state of a payment.
        /// </summary>
        /// <param name="id">The ID of the payment.</param>
        /// <param name="trigger">The trigger to change the payment state.</param>
        /// <returns>The result of the state change operation.</returns>
        public async Task<IActionResult> ChangePaymentState(long id, [FromBody] PaymentTrigger trigger)
        {
            var result = await _paymentService.ChangePaymentStateAsync(id, trigger);
            if (result.Status)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Get the current state of a payment
        /// </summary>
        [HttpGet("{id}/state")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        /// <summary>
        /// Gets the current state of a payment.
        /// </summary>
        /// <param name="id">The ID of the payment.</param>
        /// <returns>The current state of the payment.</returns>
        public async Task<IActionResult> GetPaymentState(long id)
        {
            var state = await _paymentService.GetPaymentStateAsync(id);
            if (state == null)
                return NotFound();
            if (!state.Status && state.StatusCode == ResponseStatusCode.NoRecordFound)
                return NotFound(state);
            if (!state.Status)
                return BadRequest(state);
            return Ok(state);
        }

        /// <summary>
        /// Get available triggers for the current state of a payment
        /// </summary>
        [HttpGet("{id}/triggers")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        /// <summary>
        /// Gets available triggers for the current state of a payment.
        /// </summary>
        /// <param name="id">The ID of the payment.</param>
        /// <returns>The available triggers for the payment.</returns>
        public async Task<IActionResult> GetAvailablePaymentTriggers(long id)
        {
            var triggers = await _paymentService.GetAvailableTriggersAsync(id);
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
