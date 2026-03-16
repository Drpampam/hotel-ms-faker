namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for requesting policy groups for a tenant.
    /// </summary>
    public class GetPolicyGroupsRequestDTO
    {
        /// <summary>
        /// Gets or sets the tenant ID for which to retrieve policy groups.
        /// </summary>
        public long TenantId { get; set; }
    }
}
