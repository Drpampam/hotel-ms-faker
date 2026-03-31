using hotelier_core_app.Core.States;
using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("ServiceRequest")]
    [TableName("ServiceRequest")]
    [Serializable]
    public class ServiceRequest : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(150)]
        public string? ServiceType { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // State machine properties
        [StringLength(50)]
        public ServiceRequestState ServiceRequestState { get; set; } = ServiceRequestState.Requested;
        [NotMapped]
        public Stateless.StateMachine<ServiceRequestState, ServiceRequestTrigger>? StateMachine { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(200)]
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        public string? ModifiedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey("Reservation")]
        public long ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public void ConfigureStateMachine()
        {
            StateMachine = new Stateless.StateMachine<ServiceRequestState, ServiceRequestTrigger>(() => ServiceRequestState, s => ServiceRequestState = s);
            StateMachine.Configure(ServiceRequestState.Requested)
                .Permit(ServiceRequestTrigger.Start, ServiceRequestState.InProgress)
                .Permit(ServiceRequestTrigger.Cancel, ServiceRequestState.Cancelled);
            StateMachine.Configure(ServiceRequestState.InProgress)
                .Permit(ServiceRequestTrigger.Complete, ServiceRequestState.Completed)
                .Permit(ServiceRequestTrigger.Cancel, ServiceRequestState.Cancelled);
            StateMachine.Configure(ServiceRequestState.Completed);
            StateMachine.Configure(ServiceRequestState.Cancelled);
        }
    }
}
