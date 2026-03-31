namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetServiceRequestsInputDTO
    {
        public long? ReservationId { get; set; }
        public string? ServiceType { get; set; }
        public string? State { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
