using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a discount.
    /// </summary>
    [Table("Discount")]
    [TableName("Discount")]
    [Serializable]
    public class Discount : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the discount.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the discount for identification.
        /// </summary>
        [StringLength(255)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the discount.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        /// <summary>
        /// Gets or sets the optional fixed amount discount.
        /// </summary>
        public decimal? FixedAmount { get; set; }

        /// <summary>
        /// Gets or sets the start date of the discount.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the discount.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the minimum stay days for eligibility.
        /// </summary>
        public int? MinimumStayDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum stay days for eligibility.
        /// </summary>
        public int? MaximumStayDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the discount is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the discount.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the discount.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsDeleted { get; set; }


        [ForeignKey("Tenant")]
        public long? TenantId { get; set; } // Discount applicable to a specific hotel
        /// <summary>
        /// Gets or sets the tenant entity associated with the discount.
        /// </summary>
        public Tenant? Tenant { get; set; }

        [ForeignKey("Property")]
        public long? PropertyId { get; set; } // Discount applicable to a specific property
        /// <summary>
        /// Gets or sets the property entity associated with the discount.
        /// </summary>
        public Property? Property { get; set; }

        [ForeignKey("Room")]
        public long? RoomId { get; set; } // Discount applicable to a specific room
        /// <summary>
        /// Gets or sets the room entity associated with the discount.
        /// </summary>
        public Room? Room { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>(); // Discounts linked to reservations
        public ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>(); // Discounts linked to subscription plans
    }
}
