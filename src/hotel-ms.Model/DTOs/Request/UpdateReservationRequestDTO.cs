using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class UpdateReservationRequestDTO
    {
        [Required]
        public long Id { get; set; }

        public long? RoomId { get; set; }

        public DateTime? CheckInDate { get; set; }

        public DateTime? CheckOutDate { get; set; }

        public long? DiscountId { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }
    }
}
