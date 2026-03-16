using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("Tenant")]
    [TableName("Tenant")]
    [Serializable]
    /// <summary>
    /// Represents a tenant entity in the system.
    /// </summary>
    public class Tenant : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        /// <summary>
        /// Gets or sets the unique identifier for the tenant.
        /// </summary>
        public long Id { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the name of the tenant.
        /// </summary>
        public string? Name { get; set; }

        [StringLength(255)]
        /// <summary>
        /// Gets or sets the logo URL of the tenant.
        /// </summary>
        public string? Logo { get; set; }

        [StringLength(500)]
        /// <summary>
        /// Gets or sets the description of the tenant.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Gets or sets the trial start date for the tenant.
        /// </summary>
        public DateTime? TrialStartDate { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the creator of the tenant record.
        /// </summary>
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the last modifier of the tenant record.
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the subscription start date for the tenant.
        /// </summary>
        public DateTime? SubscriptionStartDate { get; set; }
        /// <summary>
        /// Gets or sets the subscription end date for the tenant.
        /// </summary>
        public DateTime? SubscriptionEndDate { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the tenant record.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Gets or sets the last modification date of the tenant record.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the tenant is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        [ForeignKey("SubscriptionPlan")]
        /// <summary>
        /// Gets or sets the subscription plan ID associated with the tenant.
        /// </summary>
        public long? SubscriptionPlanId { get; set; }
        /// <summary>
        /// Gets or sets the subscription plan entity associated with the tenant.
        /// </summary>
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        /// <summary>
        /// Gets or sets the collection of users associated with the tenant.
        /// </summary>
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        /// <summary>
        /// Gets or sets the collection of roles associated with the tenant.
        /// </summary>
        public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
        /// <summary>
        /// Gets or sets the collection of properties associated with the tenant.
        /// </summary>
        public ICollection<Property> Properties { get; set; } = new List<Property>();
        /// <summary>
        /// Gets or sets the collection of policy groups associated with the tenant.
        /// </summary>
        public ICollection<PolicyGroup> PolicyGroups { get; set; } = new List<PolicyGroup>();
    }
}
