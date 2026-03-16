namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for user login requests.
    /// </summary>
    public class UserLoginRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the user account.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remember the user on this device.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
