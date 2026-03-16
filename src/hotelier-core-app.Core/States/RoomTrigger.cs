namespace hotelier_core_app.Core.States
{
    /// <summary>
    /// Specifies the possible triggers for room state transitions.
    /// </summary>
    public enum RoomTrigger
    {
        /// <summary>
        /// Trigger to check in to the room.
        /// </summary>
        CheckIn,
        /// <summary>
        /// Trigger to check out of the room.
        /// </summary>
        CheckOut,
        /// <summary>
        /// Trigger to set the room to cleaning.
        /// </summary>
        SetCleaning,
        /// <summary>
        /// Trigger to finish cleaning the room.
        /// </summary>
        FinishCleaning,
        /// <summary>
        /// Trigger to set the room to maintenance.
        /// </summary>
        SetMaintenance,
        /// <summary>
        /// Trigger to finish maintenance on the room.
        /// </summary>
        FinishMaintenance
    }
}
