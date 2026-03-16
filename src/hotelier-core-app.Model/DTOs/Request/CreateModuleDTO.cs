using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for creating a new module.
    /// </summary>
    public class CreateModuleDTO
    {
        /// <summary>
        /// Gets or sets the ID of the module group to which the module belongs.
        /// </summary>
        [Range(1, Int32.MaxValue)]
        public int ModuleGroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        [Required]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the module.
        /// </summary>
        [Required]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the URL associated with the module.
        /// </summary>
        public string? Url { get; set; }
    }
}
