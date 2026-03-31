using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetReservationsInputDTO
    {
        public long? GuestId { get; set; }
        public long? RoomId { get; set; }
        public long? PropertyId { get; set; }
        public ReservationState? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
