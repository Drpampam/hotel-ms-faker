namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for returning policy module permission information.
    /// </summary>
    public class GetPolicyModulePermissionDTO
    {
        /// <summary>
        /// Gets or sets the ID of the policy module permission.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the policy group.
        /// </summary>
        public long PolicyGroupId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the module group.
        /// </summary>
        public long ModuleGroupId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the permission.
        /// </summary>
        public long PermissionId { get; set; }
    }
}
