namespace hotelier_core_app.Core.States
{
    /// <summary>
    /// Specifies the possible triggers for service request state transitions.
    /// </summary>
    public enum ServiceRequestTrigger
    {
        /// <summary>
        /// Trigger to start the service request.
        /// </summary>
        Start,
        /// <summary>
        /// Trigger to complete the service request.
        /// </summary>
        Complete,
        /// <summary>
        /// Trigger to cancel the service request.
        /// </summary>
        Cancel
    }
}
