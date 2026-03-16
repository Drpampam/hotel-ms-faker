using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for room state information.
    /// </summary>
    public class RoomStateResponseDTO
    {
        /// <summary>
        /// Gets or sets the ID of the room.
        /// </summary>
        public long RoomId { get; set; }

        /// <summary>
        /// Gets or sets the current state of the room.
        /// </summary>
        public RoomState State { get; set; }

        /// <summary>
        /// Gets or sets the list of available triggers for the room state.
        /// </summary>
        public List<RoomTrigger> AvailableTriggers { get; set; } = new();
    }
}
