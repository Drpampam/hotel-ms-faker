namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetRoomsInputDTO
    {
        public long? PropertyId { get; set; }
        public string? Type { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? MaxPrice { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
