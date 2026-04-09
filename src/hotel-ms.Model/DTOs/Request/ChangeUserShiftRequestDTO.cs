using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class ChangeUserShiftRequestDTO
    {
        [Required][EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>Morning | Afternoon | Night</summary>
        public string? Shift { get; set; }

        public string? Department { get; set; }
    }
}
