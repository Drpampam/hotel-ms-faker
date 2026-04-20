using hotelier_core_app.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class SelfRegisterRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string HotelName { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public PlanType PlanType { get; set; }
    }

    public class ActivateMyAccountRequestDTO
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
