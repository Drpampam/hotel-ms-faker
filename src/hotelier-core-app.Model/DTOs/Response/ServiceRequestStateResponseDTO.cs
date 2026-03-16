using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for service request state information.
    /// </summary>
    public class ServiceRequestStateResponseDTO
    {
        /// <summary>
        /// Gets or sets the ID of the service request.
        /// </summary>
        public long ServiceRequestId { get; set; }

        /// <summary>
        /// Gets or sets the current state of the service request.
        /// </summary>
        public ServiceRequestState State { get; set; }

        /// <summary>
        /// Gets or sets the list of available triggers for the service request state.
        /// </summary>
        public List<ServiceRequestTrigger> AvailableTriggers { get; set; } = new();
    }
}
