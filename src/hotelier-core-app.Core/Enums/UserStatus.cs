namespace hotelier_core_app.Core.Enums
{
    /// <summary>
    /// Specifies the possible statuses for a user account.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// User is active.
        /// </summary>
        Active = 1,
        /// <summary>
        /// User is suspended.
        /// </summary>
        Suspended,
        /// <summary>
        /// User is sacked.
        /// </summary>
        Sacked,
        /// <summary>
        /// User has resigned.
        /// </summary>
        Resigned
    }
}
