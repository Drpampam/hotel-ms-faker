using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for editing a user's roles.
    /// </summary>
    public class EditUserRolesRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the roles to assign to the user.
        /// </summary>
        [Required]
        public IEnumerable<string>? Roles { get; set; }
    }
}
