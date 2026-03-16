using hotelier_core_app.Model.Attributes;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing the association between a user and a role.
    /// </summary>
    [Table("UserRole")]
    [TableName("UserRole")]
    [Serializable]
    public class ApplicationUserRole : IdentityUserRole<long>
    {
        /// <summary>
        /// Gets or sets the user entity associated with the role.
        /// </summary>
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        /// <summary>
        /// Gets or sets the role entity associated with the user.
        /// </summary>
        [ForeignKey("RoleId")]
        public ApplicationRole? Role { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the user-role association.
        /// </summary>
        [ForeignKey("Tenant")]
        public long TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant entity associated with the user-role association.
        /// </summary>
        public Tenant? Tenant { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserRole"/> class.
        /// </summary>
        public ApplicationUserRole() { }
    }
}
