using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("GuestProfile")]
    [TableName("GuestProfile")]
    [Serializable]
    public class GuestProfile : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        // Optional link to a platform user account. Null for walk-in guests.
        [ForeignKey("User")]
        public long? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        /// <summary>Guest's full name — required for walk-in guests; overridden by User.FullName when linked.</summary>
        [StringLength(200)]
        public string? FullName { get; set; }

        /// <summary>Guest contact email — stored directly so walk-in guests don't need a user account.</summary>
        [StringLength(200)]
        public string? Email { get; set; }

        /// <summary>Guest contact phone number.</summary>
        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? PassportNumber { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? PreferredRoomType { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        public int LoyaltyPoints { get; set; } = 0;

        [StringLength(50)]
        public string? LoyaltyTier { get; set; } = "Bronze";

        [ForeignKey("Tenant")]
        public long? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        [StringLength(200)]
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
