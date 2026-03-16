using hotelier_core_app.Core.States;
using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("Room")]
    [TableName("Room")]
    [Serializable]
    /// <summary>
    /// Represents a room entity in the system.
    /// </summary>
    public class Room : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        /// <summary>
        /// Gets or sets the unique identifier for the room.
        /// </summary>
        public long Id { get; set; }

        [StringLength(255)]
        /// <summary>
        /// Gets or sets the room number.
        /// </summary>
        public string? Number { get; set; }

        [StringLength(255)]
        /// <summary>
        /// Gets or sets the type of the room.
        /// </summary>
        public string? Type { get; set; }

        [Range(1, int.MaxValue)]
        /// <summary>
        /// Gets or sets the capacity of the room.
        /// </summary>
        public int Capacity { get; set; }
        /// <summary>
        /// Gets or sets the price per night for the room.
        /// </summary>
        public decimal PricePerNight { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the room is available.
        /// </summary>
        public bool IsAvailable { get; set; }

        // State machine properties
        [StringLength(50)]
        /// <summary>
        /// Gets or sets the current state of the room.
        /// </summary>
        public RoomState RoomState { get; set; } = RoomState.Available;
        [NotMapped]
        /// <summary>
        /// Gets or sets the state machine for the room.
        /// </summary>
        public Stateless.StateMachine<RoomState, RoomTrigger>? StateMachine { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the creator of the room record.
        /// </summary>
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the last modifier of the room record.
        /// </summary>
        public string? ModifiedBy { get; set; }
        /// <summary>
        /// Gets or sets the creation date of the room record.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Gets or sets the last modification date of the room record.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the room is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        [ForeignKey("Property")]
        /// <summary>
        /// Gets or sets the property ID associated with the room.
        /// </summary>
        public long PropertyId { get; set; }
        /// <summary>
        /// Gets or sets the property entity associated with the room.
        /// </summary>
        public Property? Property { get; set; }

        /// <summary>
        /// Gets or sets the collection of reservations associated with the room.
        /// </summary>
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        /// <summary>
        /// Configures the state machine for the room.
        /// </summary>
        public void ConfigureStateMachine()
        {
            StateMachine = new Stateless.StateMachine<RoomState, RoomTrigger>(() => RoomState, s => RoomState = s);
            StateMachine.Configure(RoomState.Available)
                .Permit(RoomTrigger.CheckIn, RoomState.Occupied)
                .Permit(RoomTrigger.SetCleaning, RoomState.Cleaning)
                .Permit(RoomTrigger.SetMaintenance, RoomState.Maintenance);
            StateMachine.Configure(RoomState.Occupied)
                .Permit(RoomTrigger.CheckOut, RoomState.Available)
                .Permit(RoomTrigger.SetCleaning, RoomState.Cleaning);
            StateMachine.Configure(RoomState.Cleaning)
                .Permit(RoomTrigger.FinishCleaning, RoomState.Available);
            StateMachine.Configure(RoomState.Maintenance)
                .Permit(RoomTrigger.FinishMaintenance, RoomState.Available);
        }
    }
}
