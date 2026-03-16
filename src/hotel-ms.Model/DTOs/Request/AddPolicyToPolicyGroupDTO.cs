namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for adding a policy to a policy group.
    /// </summary>
    public class AddPolicyToPolicyGroupDTO
    {
        /// <summary>
        /// Gets or sets the ID of the policy group.
        /// </summary>
        public long PolicyGroupId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the permission to add.
        /// </summary>
        public long PermissionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the module group associated with the policy.
        /// </summary>
        public long ModuleGroupId { get; set; }
    }
}
