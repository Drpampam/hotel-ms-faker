namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Data transfer object for module group information.
    /// </summary>
    public class ModuleGroupDTO
    {
        /// <summary>
        /// Gets or sets the ID of the module group.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the module group.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the module group.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the URL associated with the module group.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the list of modules in the group.
        /// </summary>
        public List<ModuleDTO>? Modules { get; set; }
    }
}
