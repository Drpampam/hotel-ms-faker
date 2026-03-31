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

        [ForeignKey("User")]
        public long UserId { get; set; }
        public ApplicationUser? User { get; set; }

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
