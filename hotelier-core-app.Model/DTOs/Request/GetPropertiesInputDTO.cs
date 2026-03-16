namespace hotelier_core_app.Model.DTOs.Request
{
    public class GetPropertiesInputDTO : PageParamsDTO
    {
        public long? TenantId { get; set; }
        public string? Name { get; set; }
    }
}
