namespace hotelier_core_app.Model.DTOs.Response
{
    public class ExpenseReportItemDTO
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? RoomNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class ExpenseCategorySummary
    {
        public string Category { get; set; } = string.Empty;
        public long Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExpenseReportDTO
    {
        public List<ExpenseReportItemDTO> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ExpenseCategorySummary> ByCategory { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
