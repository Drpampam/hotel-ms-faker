namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for specifying pagination parameters.
    /// </summary>
    [Serializable]
    public class PageParamsDTO
    {
        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Optional tenant filter — used by the master admin to scope results to a specific tenant.
        /// Regular users are always scoped to their own tenant via JWT.
        /// </summary>
        public long? TenantId { get; set; }
    }

}
