namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for updating a policy group.
    /// </summary>
    public class UpdatePolicyGroupDTO
    {
        /// <summary>
        /// Gets or sets the ID of the policy group to update.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the policy group.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the policy group.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the policy group.
        /// </summary>
        public long TenantId { get; set; }
    }
}
