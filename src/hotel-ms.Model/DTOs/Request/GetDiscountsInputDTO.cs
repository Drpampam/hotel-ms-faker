namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetDiscountsInputDTO
    {
        public long? TenantId { get; set; }
        public long? PropertyId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
