using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a policy group.
    /// </summary>
    [Table("PolicyGroup")]
    [TableName("PolicyGroup")]
    [Serializable]
    public class PolicyGroup : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the policy group.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the policy group.
        /// </summary>
        [StringLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the policy group.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the policy group.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the policy group.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the policy group.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the policy group.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the policy group is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the policy group.
        /// </summary>
        [ForeignKey("Tenant")]
        public long TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant entity associated with the policy group.
        /// </summary>
        public Tenant? Tenant { get; set; }

        /// <summary>
        /// Gets or sets the collection of module permissions in the policy group.
        /// </summary>
        public ICollection<PolicyModulePermission>? ModulePermissions { get; set; } = new List<PolicyModulePermission>();

        /// <summary>
        /// Gets or sets the collection of user-policy group associations.
        /// </summary>
        public ICollection<ApplicationUserPolicyGroup>? UserPolicyGroups { get; set; } = new List<ApplicationUserPolicyGroup>();

        /// <summary>
        /// Gets or sets the collection of role-policy group associations.
        /// </summary>
        public ICollection<RolePolicyGroup>? RolePolicyGroups { get; set; } = new List<RolePolicyGroup>();
    }
}
