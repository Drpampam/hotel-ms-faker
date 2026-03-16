namespace hotelier_core_app.Core.States
{
    /// <summary>
    /// Specifies the possible states for a service request.
    /// </summary>
    public enum ServiceRequestState
    {
        /// <summary>
        /// Service request has been made.
        /// </summary>
        Requested,
        /// <summary>
        /// Service request is in progress.
        /// </summary>
        InProgress,
        /// <summary>
        /// Service request is completed.
        /// </summary>
        Completed,
        /// <summary>
        /// Service request has been cancelled.
        /// </summary>
        Cancelled
    }
}
