namespace hotelier_core_app.Core.States
{
    /// <summary>
    /// Specifies the possible triggers for payment state transitions.
    /// </summary>
    public enum PaymentTrigger
    {
        /// <summary>
        /// Trigger to process the payment.
        /// </summary>
        Process,
        /// <summary>
        /// Trigger to complete the payment.
        /// </summary>
        Complete,
        /// <summary>
        /// Trigger to mark the payment as failed.
        /// </summary>
        Fail,
        /// <summary>
        /// Trigger to retry the payment.
        /// </summary>
        Retry,
        /// <summary>
        /// Trigger to refund the payment.
        /// </summary>
        Refund
    }
}
