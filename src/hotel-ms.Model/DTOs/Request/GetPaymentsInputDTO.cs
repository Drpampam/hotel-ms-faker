namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetPaymentsInputDTO
    {
        public long? ReservationId { get; set; }
        public string? PaymentMethod { get; set; }
        public bool? IsSuccessful { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
