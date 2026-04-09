using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class DeleteUserRequestDTO
    {
        [Required][EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
