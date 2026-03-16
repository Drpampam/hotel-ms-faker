namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for refresh token response.
    /// </summary>
    public class RefreshTokenResponseDTO
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
        /// Gets or sets the list of roles assigned to the user.
        /// </summary>
        public List<string>? Roles { get; set; }
    }
}
