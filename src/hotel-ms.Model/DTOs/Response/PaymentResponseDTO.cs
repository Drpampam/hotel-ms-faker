using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class PaymentResponseDTO
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public bool IsSuccessful { get; set; }
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentState PaymentState { get; set; }
    }
}
