using hotelier_core_app.Model.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("ReservationExpense")]
    [TableName("ReservationExpense")]
    [Serializable]
    public class ReservationExpense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Reservation")]
        public long ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Category { get; set; }  // Food, Laundry, Minibar, Transport, Spa, Other

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        public decimal Amount { get; set; }  // Quantity * UnitPrice

        [StringLength(200)]
        public string? CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
