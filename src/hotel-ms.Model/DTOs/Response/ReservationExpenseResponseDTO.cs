namespace hotelier_core_app.Model.DTOs.Response
{
    public class ReservationExpenseResponseDTO
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
