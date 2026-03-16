using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for editing a user's name.
    /// </summary>
    public class EditUserNameRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the new name for the user.
        /// </summary>
        [Required]
        public string? Name { get; set; }
    }
}
