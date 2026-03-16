namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for editing an existing module group.
    /// </summary>
    public class EditModuleGroupDTO
    {
        /// <summary>
        /// Gets or sets the ID of the module group to edit.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the module group.
        /// </summary>
        public string? Name { get; set; } // Confirming nullable property

        /// <summary>
        /// Gets or sets the description of the module group.
        /// </summary>
        public string? Description { get; set; } // Confirming nullable property

        /// <summary>
        /// Gets or sets the URL associated with the module group.
        /// </summary>
        public string? Url { get; set; } // Confirming nullable property
    }
}
