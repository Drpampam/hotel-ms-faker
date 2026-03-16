using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for requesting a refresh token.
    /// </summary>
    public class RefreshTokenRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the refresh token value.
        /// </summary>
        [Required]
        public string? RefreshToken { get; set; }
    }
}
