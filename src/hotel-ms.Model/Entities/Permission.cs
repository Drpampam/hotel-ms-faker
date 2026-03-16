using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a permission.
    /// </summary>
    [Table("Permission")]
    [TableName("Permission")]
    [Serializable]
    public class Permission : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the permission.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the permission.
        /// </summary>
        [StringLength(255)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the permission.
        /// </summary>
        [StringLength(255)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the permission.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the permission.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the permission.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the permission.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the permission is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
