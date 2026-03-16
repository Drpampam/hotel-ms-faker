using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing the association between a policy group, module group, and permission.
    /// </summary>
    [Table("PolicyModulePermission")]
    [TableName("PolicyModulePermission")]
    [Serializable]
    public class PolicyModulePermission : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the policy-module-permission association.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the association.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the association.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the association.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the association.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the association is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the policy group ID associated with the association.
        /// </summary>
        [ForeignKey("PolicyGroup")]
        public long PolicyGroupId { get; set; }

        /// <summary>
        /// Gets or sets the policy group entity associated with the association.
        /// </summary>
        public PolicyGroup? PolicyGroup { get; set; }

        /// <summary>
        /// Gets or sets the module group ID associated with the association.
        /// </summary>
        [ForeignKey("ModuleGroup")]
        public long ModuleGroupId { get; set; }

        /// <summary>
        /// Gets or sets the module group entity associated with the association.
        /// </summary>
        public ModuleGroup? ModuleGroup { get; set; }

        /// <summary>
        /// Gets or sets the permission ID associated with the association.
        /// </summary>
        [ForeignKey("Permission")]
        public long PermissionId { get; set; }

        /// <summary>
        /// Gets or sets the permission entity associated with the association.
        /// </summary>
        public Permission? Permission { get; set; }
    }
}
