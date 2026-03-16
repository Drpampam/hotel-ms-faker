namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for role information.
    /// </summary>
    public class RoleDTO
    {
        /// <summary>
        /// Gets or sets the ID of the role.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        public string? Name { get; set; }
    }
}
