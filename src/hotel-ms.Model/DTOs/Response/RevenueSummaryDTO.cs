namespace hotelier_core_app.Model.DTOs.Response
{
    public class RevenueSummaryDTO
    {
        public decimal TotalRevenue { get; set; }
        public decimal RoomRevenue { get; set; }
        public decimal TaxCollected { get; set; }
        public decimal TotalDiscountsApplied { get; set; }
        public int PaidInvoicesCount { get; set; }
        public int PendingInvoicesCount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
