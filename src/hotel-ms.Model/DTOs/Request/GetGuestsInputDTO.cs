namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetGuestsInputDTO
    {
        public long? TenantId { get; set; }
        public string? SearchTerm { get; set; }
        public string? Nationality { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
