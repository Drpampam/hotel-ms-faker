using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class ActivateTenantRequestDTO
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string TenantName { get; set; } = string.Empty;

        [Required]
        public string AdminPassword { get; set; } = string.Empty;

        [Required]
        public string AdminFullName { get; set; } = string.Empty;
    }
}
