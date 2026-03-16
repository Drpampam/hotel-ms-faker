using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for editing user details.
    /// </summary>
    public class EditUserDetailRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        [Required]
        public string? FullName { get; set; }

        /// <summary>
        /// Gets or sets the roles assigned to the user.
        /// </summary>
        [Required]
        public IEnumerable<string>? Roles { get; set; }
    }
}
