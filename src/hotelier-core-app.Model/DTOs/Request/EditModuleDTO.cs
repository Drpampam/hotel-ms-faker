namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for editing an existing module.
    /// </summary>
    public class EditModuleDTO
    {
        /// <summary>
        /// Gets or sets the ID of the module to edit.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the module.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the URL associated with the module.
        /// </summary>
        public string? Url { get; set; }
    }
}
