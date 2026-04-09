using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing an application user.
    /// </summary>
    [Table("User")]
    [TableName("User")]
    [Serializable]
    public class ApplicationUser : IdentityUser<long>, IBaseEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the row version for concurrency control.
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        [StringLength(200)]
        public string? FullName { get; set; }

        /// <summary>
        /// Gets or sets the status of the user.
        /// </summary>
        [StringLength(200)]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the account.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the account.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the user account.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the user account.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the profile picture URL of the user.
        /// </summary>
        public string? Picture { get; set; }

        /// <summary>
        /// Gets or sets the refresh token for the user.
        /// </summary>
        [StringLength(200)]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the user role associated with the user.
        /// </summary>
        public ApplicationUserRole? UserRole { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the user.
        /// </summary>
        /// <summary>
        /// Gets or sets the tenant ID associated with the user.
        /// </summary>
        [ForeignKey("Tenant")]
        public long? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant entity associated with the user.
        /// </summary>
        public Tenant? Tenant { get; set; }
        [StringLength(50)]
        public string? Shift { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public ICollection<ApplicationUserPolicyGroup> UserPolicyGroups { get; set; } = new List<ApplicationUserPolicyGroup>();

        public ApplicationUser()
        {
            CreationDate = DateTime.UtcNow;
            IsDeleted = false;
            IsActive = true;
            Status = "Active";
        }
    }
}
