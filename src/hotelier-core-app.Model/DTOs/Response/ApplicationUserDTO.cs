namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object representing an application user.
    /// </summary>
    public class ApplicationUserDTO
    {
        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Gets or sets the status of the user.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the user account.
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last active date of the user.
        /// </summary>
        public DateTime? LastActiveDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the user account.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the list of roles assigned to the user.
        /// </summary>
        public List<RoleDTO>? UserRoles { get; set; }
    }
}
