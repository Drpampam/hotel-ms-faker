using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class ReservationResponseDTO
    {
        public long Id { get; set; }
        public long RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public long GuestId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NightsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ExpensesTotal { get; set; }
        public decimal GrandTotal => TotalPrice + ExpensesTotal;
        public ReservationState Status { get; set; }
        public string? SpecialRequests { get; set; }
        public long? DiscountId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public List<ReservationExpenseResponseDTO> Expenses { get; set; } = new();
    }
}
