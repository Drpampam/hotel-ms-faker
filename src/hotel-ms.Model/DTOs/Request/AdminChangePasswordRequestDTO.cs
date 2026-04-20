using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class AdminChangePasswordRequestDTO
    {
        [Required][EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required][MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangeTempPasswordRequestDTO
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required][MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
