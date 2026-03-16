using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.States;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation
{
    /// <summary>
    /// Provides business logic for managing payments and payment state transitions.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IDBCommandRepository<Payment> _paymentCommandRepository;
        private readonly IDBQueryRepository<Payment> _paymentQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IMapper _mapper;

        public PaymentService(
            IDBCommandRepository<Payment> paymentCommandRepository,
            IDBQueryRepository<Payment> paymentQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IMapper mapper)
        {
            _paymentCommandRepository = paymentCommandRepository;
            _paymentQueryRepository = paymentQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the current state and available triggers for a payment.
        /// </summary>
        /// <param name="paymentId">The ID of the payment.</param>
        /// <returns>Returns the payment state and available triggers, or failure if not found.</returns>
        public async Task<BaseResponse<PaymentStateResponseDTO>> GetPaymentStateAsync(long paymentId)
        {
            var payment = await _paymentQueryRepository.FindAsync(paymentId);
            if (payment == null)
                return BaseResponse<PaymentStateResponseDTO>.Failure(new PaymentStateResponseDTO(), "Payment not found", ResponseStatusCode.NoRecordFound);
            payment.ConfigureStateMachine();
            var triggers = payment.StateMachine != null ? (await payment.StateMachine.PermittedTriggersAsync).ToList() : new List<PaymentTrigger>();
            var responseDto = new PaymentStateResponseDTO
            {
                PaymentId = payment.Id,
                State = payment.PaymentState,
                AvailableTriggers = triggers
            };
            return BaseResponse<PaymentStateResponseDTO>.Success(responseDto, "State fetched successfully", ResponseStatusCode.OperationSuccessful);
        }

        /// <summary>
        /// Changes the state of a payment using the specified trigger.
        /// </summary>
        /// <param name="paymentId">The ID of the payment.</param>
        /// <param name="trigger">The trigger to fire for the state change.</param>
        /// <returns>Returns the updated payment state, or failure if not found or trigger is invalid.</returns>
        public async Task<BaseResponse<PaymentStateResponseDTO>> ChangePaymentStateAsync(long paymentId, PaymentTrigger trigger)
        {
            var payment = await _paymentQueryRepository.FindAsync(paymentId);
            if (payment == null)
                return BaseResponse<PaymentStateResponseDTO>.Failure(new PaymentStateResponseDTO(), "Payment not found", ResponseStatusCode.NoRecordFound);
            payment.ConfigureStateMachine();
            if (payment.StateMachine == null || !payment.StateMachine.CanFire(trigger))
                return BaseResponse<PaymentStateResponseDTO>.Failure(new PaymentStateResponseDTO(), "Invalid trigger", ResponseStatusCode.InvalidData);
            payment.StateMachine.Fire(trigger);
            await _paymentCommandRepository.UpdateAsync(payment);
            var triggers = (await payment.StateMachine.PermittedTriggersAsync).ToList();
            var responseDto = new PaymentStateResponseDTO
            {
                PaymentId = payment.Id,
                State = payment.PaymentState,
                AvailableTriggers = triggers
            };
            return BaseResponse<PaymentStateResponseDTO>.Success(responseDto, "State changed successfully", ResponseStatusCode.OperationSuccessful);
        }

        /// <summary>
        /// Retrieves all available triggers for a payment's current state.
        /// </summary>
        /// <param name="paymentId">The ID of the payment.</param>
        /// <returns>Returns a list of available triggers, or failure if payment not found.</returns>
        public async Task<BaseResponse<List<PaymentTrigger>>> GetAvailableTriggersAsync(long paymentId)
        {
            var payment = await _paymentQueryRepository.FindAsync(paymentId);
            if (payment == null)
                return BaseResponse<List<PaymentTrigger>>.Failure(new List<PaymentTrigger>(), "Payment not found", ResponseStatusCode.NoRecordFound);
            payment.ConfigureStateMachine();
            var triggers = payment.StateMachine != null ? (await payment.StateMachine.PermittedTriggersAsync).ToList() : new List<PaymentTrigger>();
            return BaseResponse<List<PaymentTrigger>>.Success(triggers, "Available triggers fetched successfully", ResponseStatusCode.OperationSuccessful);
        }
    }
}
