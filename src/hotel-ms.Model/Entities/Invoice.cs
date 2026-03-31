using hotelier_core_app.Core.States;
using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("Invoice")]
    [TableName("Invoice")]
    [Serializable]
    public class Invoice : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(50)]
        public string? InvoiceNumber { get; set; }

        [ForeignKey("Reservation")]
        public long ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [ForeignKey("Guest")]
        public long GuestId { get; set; }
        public ApplicationUser? Guest { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        [ForeignKey("Tenant")]
        public long? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();

        [StringLength(200)]
        public string? CreatedBy { get; set; }
        [StringLength(200)]
        public string? ModifiedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
