using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("Reservation")]
    [TableName("Reservation")]
    [Serializable]
    /// <summary>
    /// Represents a reservation entity in the system.
    /// </summary>
    public class Reservation : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        /// <summary>
        /// Gets or sets the unique identifier for the reservation.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the check-in date for the reservation.
        /// </summary>
        public DateTime CheckInDate { get; set; }
        /// <summary>
        /// Gets or sets the check-out date for the reservation.
        /// </summary>
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Gets or sets the total price for the reservation.
        /// </summary>
        public decimal TotalPrice { get; set; }

        [StringLength(255)]
        /// <summary>
        /// Gets or sets the status of the reservation.
        /// </summary>
        public string? Status { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the creator of the reservation record.
        /// </summary>
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the last modifier of the reservation record.
        /// </summary>
        public string? ModifiedBy { get; set; }
        /// <summary>
        /// Gets or sets the creation date of the reservation record.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Gets or sets the last modification date of the reservation record.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the reservation is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        [ForeignKey("User")]
        /// <summary>
        /// Gets or sets the guest ID associated with the reservation.
        /// </summary>
        public long GuestId { get; set; }
        /// <summary>
        /// Gets or sets the user entity associated with the reservation.
        /// </summary>
        public ApplicationUser? User { get; set; }

        [ForeignKey("Room")]
        /// <summary>
        /// Gets or sets the room ID associated with the reservation.
        /// </summary>
        public long RoomId { get; set; }
        /// <summary>
        /// Gets or sets the room entity associated with the reservation.
        /// </summary>
        public Room? Room { get; set; }

        [ForeignKey("Discount")]
        /// <summary>
        /// Gets or sets the discount ID associated with the reservation.
        /// </summary>
        public long? DiscountId { get; set; }
        /// <summary>
        /// Gets or sets the discount entity associated with the reservation.
        /// </summary>
        public Discount? Discount { get; set; }
    }
}
