using hotelier_core_app.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class GenerateActivationCodeRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public PlanType PlanType { get; set; }
    }
}
