namespace hotelier_core_app.Model.DTOs.Response
{
    public class TenantProvisionResponseDTO
    {
        public long TenantId { get; set; }
        public string Schema { get; set; } = string.Empty;
        public bool IsProvisioned { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
