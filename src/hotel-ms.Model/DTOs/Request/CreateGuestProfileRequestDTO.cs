using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class CreateGuestProfileRequestDTO
    {
        /// <summary>Guest's full name. Required for walk-in guests; optional when UserId is provided.</summary>
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Guest contact email (optional).</summary>
        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        /// <summary>Guest contact phone number (optional).</summary>
        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        /// <summary>Optional link to an existing platform user account.</summary>
        public long? UserId { get; set; }

        [StringLength(100)]
        public string? PassportNumber { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? PreferredRoomType { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        public long? TenantId { get; set; }
    }
}
