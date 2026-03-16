using hotelier_core_app.Core.Enums;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for deactivating a user account.
    /// </summary>
    public class DeactivateUserRequestDTO : ActivateUserRequestDTO
    {
        /// <summary>
        /// Gets or sets the status to assign to the user upon deactivation.
        /// </summary>
        public UserStatus Status { get; set; }
    }
}
