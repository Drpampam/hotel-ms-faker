namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for adding a user to a policy group.
    /// </summary>
    public class AddUserToPolicyGroupDTO
    {
        /// <summary>
        /// Gets or sets the ID of the user to add.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the policy group to add the user to.
        /// </summary>
        public long PolicyGroupId { get; set; }
    }
}
