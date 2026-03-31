using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class CreateReservationRequestDTO
    {
        [Required]
        public long RoomId { get; set; }

        [Required]
        public long GuestId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public long? DiscountId { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }
    }
}
