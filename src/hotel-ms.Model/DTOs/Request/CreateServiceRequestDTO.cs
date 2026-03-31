using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class CreateServiceRequestDTO
    {
        [Required]
        public long ReservationId { get; set; }

        [Required]
        [StringLength(150)]
        public string ServiceType { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
