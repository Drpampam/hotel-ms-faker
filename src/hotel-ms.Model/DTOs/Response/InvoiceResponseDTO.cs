using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class InvoiceResponseDTO
    {
        public long Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public long ReservationId { get; set; }
        public long GuestId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public List<InvoiceLineItemDTO>? LineItems { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class InvoiceLineItemDTO
    {
        public long Id { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
