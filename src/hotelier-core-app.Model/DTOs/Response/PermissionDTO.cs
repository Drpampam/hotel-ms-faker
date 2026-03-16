namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for permission information.
    /// </summary>
    public class PermissionDTO
    {
        /// <summary>
        /// Gets or sets the ID of the permission.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the permission.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the permission.
        /// </summary>
        public string? Description { get; set; }
    }
}
