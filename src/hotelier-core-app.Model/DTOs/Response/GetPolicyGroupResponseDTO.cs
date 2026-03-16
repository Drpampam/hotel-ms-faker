namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for returning policy group information.
    /// </summary>
    public class GetPolicyGroupResponseDTO
    {
        /// <summary>
        /// Gets or sets the ID of the policy group.
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
        /// Gets or sets the creation date of the policy group.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the policy group.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the policy group.
        /// </summary>
        public long TenantId { get; set; }
    }
}
