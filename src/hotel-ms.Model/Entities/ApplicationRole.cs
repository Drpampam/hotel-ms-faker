using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing an application role.
    /// </summary>
    [Table("Role")]
    [TableName("Role")]
    [Serializable]
    public class ApplicationRole : IdentityRole<long>, IBaseEntity
    {
        /// <summary>
        /// Gets or sets the name of the user who created the role.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the role.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the role.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the role.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the role.
        /// </summary>
        [ForeignKey("Tenant")]
        public long? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant entity associated with the role.
        /// </summary>
        public Tenant? Tenant { get; set; }

        /// <summary>
        /// Gets or sets the collection of role policy groups associated with the role.
        /// </summary>
        public ICollection<RolePolicyGroup> RolePolicyGroups { get; set; } = new List<RolePolicyGroup>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
        /// </summary>
        public ApplicationRole() { }
    }
}
