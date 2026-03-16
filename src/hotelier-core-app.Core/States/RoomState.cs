namespace hotelier_core_app.Core.States
{
    /// <summary>
    /// Specifies the possible states for a room.
    /// </summary>
    public enum RoomState
    {
        /// <summary>
        /// Room is available.
        /// </summary>
        Available,
        /// <summary>
        /// Room is occupied.
        /// </summary>
        Occupied,
        /// <summary>
        /// Room is being cleaned.
        /// </summary>
        Cleaning,
        /// <summary>
        /// Room is under maintenance.
        /// </summary>
        Maintenance
    }
}
