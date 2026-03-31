using hotelier_core_app.Core.States;
using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("HousekeepingTask")]
    [TableName("HousekeepingTask")]
    [Serializable]
    public class HousekeepingTask : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Room")]
        public long RoomId { get; set; }
        public Room? Room { get; set; }

        [ForeignKey("AssignedTo")]
        public long? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedTo { get; set; }

        [StringLength(100)]
        public string? TaskType { get; set; }   // Cleaning, Turndown, Inspection

        [StringLength(50)]
        public string? Priority { get; set; }    // Low, Normal, High, Urgent

        [StringLength(500)]
        public string? Notes { get; set; }

        public HousekeepingTaskState State { get; set; } = HousekeepingTaskState.Pending;

        [NotMapped]
        public Stateless.StateMachine<HousekeepingTaskState, HousekeepingTaskTrigger>? StateMachine { get; set; }

        public DateTime? ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }

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

        public void ConfigureStateMachine()
        {
            StateMachine = new Stateless.StateMachine<HousekeepingTaskState, HousekeepingTaskTrigger>(
                () => State, s => State = s);
            StateMachine.Configure(HousekeepingTaskState.Pending)
                .Permit(HousekeepingTaskTrigger.Start, HousekeepingTaskState.InProgress)
                .Permit(HousekeepingTaskTrigger.Skip, HousekeepingTaskState.Skipped);
            StateMachine.Configure(HousekeepingTaskState.InProgress)
                .Permit(HousekeepingTaskTrigger.Complete, HousekeepingTaskState.Done)
                .Permit(HousekeepingTaskTrigger.Skip, HousekeepingTaskState.Skipped);
            StateMachine.Configure(HousekeepingTaskState.Done);
            StateMachine.Configure(HousekeepingTaskState.Skipped);
        }
    }
}
