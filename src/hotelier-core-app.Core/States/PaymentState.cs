namespace hotelier_core_app.Core.States
{
    /// <summary>
    /// Specifies the possible states for a payment.
    /// </summary>
    public enum PaymentState
    {
        /// <summary>
        /// Payment is pending.
        /// </summary>
        Pending,
        /// <summary>
        /// Payment is being processed.
        /// </summary>
        Processing,
        /// <summary>
        /// Payment is completed.
        /// </summary>
        Completed,
        /// <summary>
        /// Payment has failed.
        /// </summary>
        Failed,
        /// <summary>
        /// Payment has been refunded.
        /// </summary>
        Refunded
    }
}
