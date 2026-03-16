using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a module.
    /// </summary>
    [Table("Module")]
    [TableName("Module")]
    [Serializable]
    public class Module : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the module.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        [StringLength(255)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the module.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the URL associated with the module.
        /// </summary>
        [StringLength(500)]
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the module.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the module.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the module.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the module.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the module is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the module group ID associated with the module.
        /// </summary>
        [Range(1, Int64.MaxValue)]
        public long ModuleGroupId { get; set; }

        /// <summary>
        /// Gets or sets the module group entity associated with the module.
        /// </summary>
        [ForeignKey("ModuleGroupId")]
        public ModuleGroup? ModuleGroup { get; set; }

        public Module() { }
    }
}
