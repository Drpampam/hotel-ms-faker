using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class ServiceRequestResponseDTO
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public string? ServiceType { get; set; }
        public string? Notes { get; set; }
        public ServiceRequestState ServiceRequestState { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
