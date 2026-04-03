using hotelier_core_app.Core.States;
using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("Reservation")]
    [TableName("Reservation")]
    [Serializable]
    public class Reservation : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public decimal TotalPrice { get; set; }

        public ReservationState Status { get; set; } = ReservationState.Pending;

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

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

        [ForeignKey("GuestProfile")]
        /// <summary>
        /// Gets or sets the guest profile ID associated with the reservation.
        /// </summary>
        public long GuestId { get; set; }
        /// <summary>
        /// Gets or sets the guest profile entity associated with the reservation.
        /// </summary>
        public GuestProfile? GuestProfile { get; set; }

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
