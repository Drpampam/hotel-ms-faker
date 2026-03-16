using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for creating a new module group.
    /// </summary>
    public class CreateModuleGroupDTO
    {
        /// <summary>
        /// Gets or sets the name of the module group.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the module group.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the URL associated with the module group.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string? Url { get; set; }
    }
}
