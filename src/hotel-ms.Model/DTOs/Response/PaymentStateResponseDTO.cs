using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for payment state information.
    /// </summary>
    public class PaymentStateResponseDTO
    {
        /// <summary>
        /// Gets or sets the ID of the payment.
        /// </summary>
        public long PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the current state of the payment.
        /// </summary>
        public PaymentState State { get; set; }

        /// <summary>
        /// Gets or sets the list of available triggers for the payment state.
        /// </summary>
        public List<PaymentTrigger> AvailableTriggers { get; set; } = new();
    }
}
