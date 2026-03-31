using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class UpdateRoomRequestDTO
    {
        [Required]
        public long Id { get; set; }

        [StringLength(50)]
        public string? Number { get; set; }

        [StringLength(100)]
        public string? Type { get; set; }

        [Range(1, 100)]
        public int? Capacity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price per night must be greater than 0")]
        public decimal? PricePerNight { get; set; }
    }
}
