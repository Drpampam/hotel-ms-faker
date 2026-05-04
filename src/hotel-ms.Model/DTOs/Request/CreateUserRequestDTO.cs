using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for creating a new user.
    /// </summary>
    public class CreateUserRequestDTO : UserRequestDTO
    {
        /// <summary>
        /// Gets or sets the name of the hotel associated with the user.
        /// </summary>
        public string? HotelName { get; set; }

        /// <summary>
        /// Gets or sets the subscription plan ID for the user.
        /// </summary>
        public int SubscriptionPlanId { get; set; }
    }

    /// <summary>
    /// Base data transfer object for user-related requests.
    /// </summary>
    public class UserRequestDTO
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
        /// Gets or sets the phone number of the user.
        /// </summary>
        [Required]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the password for the user account.
        /// </summary>
        [Required]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the role assigned to the user.
        /// </summary>
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
