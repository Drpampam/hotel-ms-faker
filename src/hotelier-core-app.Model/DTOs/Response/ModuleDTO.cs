namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for module information.
    /// </summary>
    public class ModuleDTO
    {
        /// <summary>
        /// Gets or sets the ID of the module.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the module group.
        /// </summary>
        public int ModuleGroupId { get; set; }

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
