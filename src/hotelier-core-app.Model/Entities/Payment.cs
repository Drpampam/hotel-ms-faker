using hotelier_core_app.Core.States;
using hotelier_core_app.Model.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a payment record.
    /// </summary>
    [Table("Payment")]
    [TableName("Payment")]
    [Serializable]
    public class Payment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the payment.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the reservation ID associated with the payment.
        /// </summary>
        [ForeignKey("Reservation")]
        public long ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the reservation entity associated with the payment.
        /// </summary>
        public Reservation? Reservation { get; set; }

        /// <summary>
        /// Gets or sets the payment method used.
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the amount paid.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID for the payment.
        /// </summary>
        [StringLength(255)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the payment.
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the current state of the payment.
        /// </summary>
        public PaymentState PaymentState { get; set; } = PaymentState.Pending;

        /// <summary>
        /// Gets or sets the state machine for payment state transitions.
        /// </summary>
        [NotMapped]
        public Stateless.StateMachine<PaymentState, PaymentTrigger>? StateMachine { get; set; }

        /// <summary>
        /// Configures the state machine for payment state transitions.
        /// </summary>
        public void ConfigureStateMachine()
        {
            StateMachine = new Stateless.StateMachine<PaymentState, PaymentTrigger>(() => PaymentState, s => PaymentState = s);
            StateMachine.Configure(PaymentState.Pending)
                .Permit(PaymentTrigger.Process, PaymentState.Processing)
                .Permit(PaymentTrigger.Fail, PaymentState.Failed);
            StateMachine.Configure(PaymentState.Processing)
                .Permit(PaymentTrigger.Complete, PaymentState.Completed)
                .Permit(PaymentTrigger.Fail, PaymentState.Failed);
            StateMachine.Configure(PaymentState.Completed)
                .Permit(PaymentTrigger.Refund, PaymentState.Refunded);
            StateMachine.Configure(PaymentState.Failed)
                .Permit(PaymentTrigger.Retry, PaymentState.Processing);
            StateMachine.Configure(PaymentState.Refunded);
        }
    }
}
