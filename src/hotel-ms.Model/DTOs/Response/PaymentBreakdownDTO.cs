namespace hotelier_core_app.Model.DTOs.Response
{
    public class PaymentBreakdownDTO
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PaymentMethodSummary> ByMethod { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class PaymentMethodSummary
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
