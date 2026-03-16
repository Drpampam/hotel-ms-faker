using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing the association between a user and a policy group.
    /// </summary>
    [Table("UserPolicyGroup")]
    [TableName("UserPolicyGroup")]
    [Serializable]
    public class ApplicationUserPolicyGroup : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user-policy group association.
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
        /// Gets or sets the user ID associated with the policy group.
        /// </summary>
        [ForeignKey("User")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the user entity associated with the policy group.
        /// </summary>
        public ApplicationUser? User { get; set; }

        /// <summary>
        /// Gets or sets the policy group ID associated with the user.
        /// </summary>
        [ForeignKey("PolicyGroup")]
        public long PolicyGroupId { get; set; }

        /// <summary>
        /// Gets or sets the policy group entity associated with the user.
        /// </summary>
        public PolicyGroup? PolicyGroup { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the user-policy group association.
        /// </summary>
        [ForeignKey("Tenant")]
        public long TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant entity associated with the user-policy group association.
        /// </summary>
        public Tenant? Tenant { get; set; }
    }
}
