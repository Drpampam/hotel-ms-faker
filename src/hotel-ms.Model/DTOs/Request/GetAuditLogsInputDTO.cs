using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetAuditLogsInputDTO : PaginationInputDTO
    {
        public string? PerformerEmail { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
