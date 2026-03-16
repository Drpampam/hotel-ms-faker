using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a module group.
    /// </summary>
    [Table("ModuleGroup")]
    [TableName("ModuleGroup")]
    [Serializable]
    public class ModuleGroup : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the module group.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the module group.
        /// </summary>
        [StringLength(255)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the module group.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the URL associated with the module group.
        /// </summary>
        [StringLength(500)]
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the module group.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the module group.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the module group.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the module group.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the module group is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the collection of modules in the group.
        /// </summary>
        public ICollection<Module>? Modules { get; set; } = new List<Module>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleGroup"/> class.
        /// </summary>
        public ModuleGroup() { }
    }
}
