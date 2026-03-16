using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("SubscriptionPlan")]
    [TableName("SubscriptionPlan")]
    [Serializable]
    /// <summary>
    /// Represents a subscription plan entity in the system.
    /// </summary>
    public class SubscriptionPlan : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        /// <summary>
        /// Gets or sets the unique identifier for the subscription plan.
        /// </summary>
        public long Id { get; set; }

        [StringLength(50)]
        /// <summary>
        /// Gets or sets the name of the subscription plan.
        /// </summary>
        public string? Name { get; set; }

        [StringLength(500)]
        /// <summary>
        /// Gets or sets the description of the subscription plan.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the price of the subscription plan.
        /// </summary>
        public decimal Price { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the creator of the subscription plan record.
        /// </summary>
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the last modifier of the subscription plan record.
        /// </summary>
        public string? ModifiedBy { get; set; }
        /// <summary>
        /// Gets or sets the creation date of the subscription plan record.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Gets or sets the last modification date of the subscription plan record.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the subscription plan is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        [ForeignKey("Discount")]
        /// <summary>
        /// Gets or sets the discount ID associated with the subscription plan.
        /// </summary>
        public long? DiscountId { get; set; }
        /// <summary>
        /// Gets or sets the discount entity associated with the subscription plan.
        /// </summary>
        public Discount? Discount { get; set; }
    }
}
