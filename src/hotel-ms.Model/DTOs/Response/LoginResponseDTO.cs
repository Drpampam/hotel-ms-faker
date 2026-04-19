namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for user login response.
    /// </summary>
    public class LoginResponseDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Gets or sets the profile picture URL of the user.
        /// </summary>
        public string? Picture { get; set; }

        /// <summary>
        /// Gets or sets the list of roles assigned to the user.
        /// </summary>
        public List<string>? Roles { get; set; }
        public long? TenantId { get; set; }
    }
}
