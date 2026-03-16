using hotelier_core_app.Model.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("RolePolicyGroup")]
    [TableName("RolePolicyGroup")]
    [Serializable]
    /// <summary>
    /// Represents the association between a role and a policy group for a tenant.
    /// </summary>
    public class RolePolicyGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        /// <summary>
        /// Gets or sets the unique identifier for the role-policy group association.
        /// </summary>
        public long Id { get; set; }

        [ForeignKey("Role")]
        /// <summary>
        /// Gets or sets the role ID associated with the policy group.
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// Gets or sets the role entity associated with the policy group.
        /// </summary>
        public ApplicationRole? Role { get; set; }

        [ForeignKey("PolicyGroup")]
        /// <summary>
        /// Gets or sets the policy group ID associated with the role.
        /// </summary>
        public long PolicyGroupId { get; set; }
        /// <summary>
        /// Gets or sets the policy group entity associated with the role.
        /// </summary>
        public PolicyGroup? PolicyGroup { get; set; }

        [ForeignKey("Tenant")]
        /// <summary>
        /// Gets or sets the tenant ID associated with the role-policy group.
        /// </summary>
        public long TenantId { get; set; }
        /// <summary>
        /// Gets or sets the tenant entity associated with the role-policy group.
        /// </summary>
        public Tenant? Tenant { get; set; }
    }
}
