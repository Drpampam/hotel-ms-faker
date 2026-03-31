namespace hotelier_core_app.Model.DTOs.Response
{
    public class AuditLogResponseDTO
    {
        public long Id { get; set; }
        public string? Action { get; set; }
        public string? PerformedBy { get; set; }
        public string? PerformerEmail { get; set; }
        public string? PerformedAgainst { get; set; }
        public string? IpAddress { get; set; }
        public DateTime DatePerformed { get; set; }
    }
}
