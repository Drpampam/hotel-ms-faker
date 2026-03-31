namespace hotelier_core_app.Model.DTOs.Response
{
    public class DiscountResponseDTO
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Percentage { get; set; }
        public decimal? FixedAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinimumStayDays { get; set; }
        public int? MaximumStayDays { get; set; }
        public bool IsActive { get; set; }
        public long? TenantId { get; set; }
        public long? PropertyId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
