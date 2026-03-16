using hotelier_core_app.Core.Enums;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for activating a user account.
    /// </summary>
    public class ActivateUserRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user to activate.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the role to assign to the user upon activation.
        /// </summary>
        public UserRole Role { get; set; }
    }
}
