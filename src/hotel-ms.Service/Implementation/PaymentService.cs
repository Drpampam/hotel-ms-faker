using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Core.States;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
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
        private readonly IDBQueryRepository<Reservation> _reservationQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;

        public PaymentService(
            IDBCommandRepository<Payment> paymentCommandRepository,
            IDBQueryRepository<Payment> paymentQueryRepository,
            IDBQueryRepository<Reservation> reservationQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IMapper mapper,
            IUtility utility)
        {
            _paymentCommandRepository = paymentCommandRepository;
            _paymentQueryRepository = paymentQueryRepository;
            _reservationQueryRepository = reservationQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _mapper = mapper;
            _utility = utility;
        }

        public async Task<BaseResponse<PaymentResponseDTO>> CreatePaymentAsync(CreatePaymentRequestDTO request, AuditLog auditLog)
        {
            var reservation = await _reservationQueryRepository.FindAsync(request.ReservationId);
            if (reservation == null)
                return BaseResponse<PaymentResponseDTO>.Failure(new PaymentResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            var payment = _mapper.Map<Payment>(request);
            payment.PaymentDate = DateTime.UtcNow;
            payment.PaymentState = PaymentState.Pending;
            payment.IsSuccessful = false;

            _paymentCommandRepository.Add(payment);
            _auditLogCommandRepository.Add(auditLog);
            await _paymentCommandRepository.SaveAsync();

            var response = _mapper.Map<PaymentResponseDTO>(payment);
            return BaseResponse<PaymentResponseDTO>.Success(response, ResponseMessages.PaymentCreated, ResponseStatusCode.PaymentCreated);
        }

        /// <summary>
        /// Creates a payment and immediately advances it to Completed — used for point-of-sale capture at checkout.
        /// </summary>
        public async Task<BaseResponse<PaymentResponseDTO>> CapturePaymentAsync(CreatePaymentRequestDTO request, AuditLog auditLog)
        {
            var reservation = await _reservationQueryRepository.FindAsync(request.ReservationId);
            if (reservation == null)
                return BaseResponse<PaymentResponseDTO>.Failure(new PaymentResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            var payment = _mapper.Map<Payment>(request);
            payment.PaymentDate = DateTime.UtcNow;
            payment.PaymentState = PaymentState.Pending;
            payment.IsSuccessful = false;

            _paymentCommandRepository.Add(payment);
            _auditLogCommandRepository.Add(auditLog);
            await _paymentCommandRepository.SaveAsync();

            // Advance Pending → Processing → Completed
            payment.ConfigureStateMachine();
            if (payment.StateMachine != null && payment.StateMachine.CanFire(PaymentTrigger.Process))
            {
                payment.StateMachine.Fire(PaymentTrigger.Process);
                if (payment.StateMachine.CanFire(PaymentTrigger.Complete))
                {
                    payment.StateMachine.Fire(PaymentTrigger.Complete);
                    payment.IsSuccessful = true;
                }
            }

            await _paymentCommandRepository.UpdateAsync(payment);

            var response = _mapper.Map<PaymentResponseDTO>(payment);
            return BaseResponse<PaymentResponseDTO>.Success(response, ResponseMessages.PaymentCreated, ResponseStatusCode.PaymentCreated);
        }

        public async Task<BaseResponse<PaymentResponseDTO>> GetPaymentByIdAsync(long paymentId)
        {
            var payment = await _paymentQueryRepository.FindAsync(paymentId);
            if (payment == null)
                return BaseResponse<PaymentResponseDTO>.Failure(new PaymentResponseDTO(), ResponseMessages.PaymentNotFound, ResponseStatusCode.PaymentNotFound);

            var response = _mapper.Map<PaymentResponseDTO>(payment);
            return BaseResponse<PaymentResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<PaymentResponseDTO>>> GetPaymentsAsync(GetPaymentsInputDTO input)
        {
            var allPayments = await _paymentQueryRepository.GetByAsync(p =>
                (!input.ReservationId.HasValue || p.ReservationId == input.ReservationId.Value) &&
                (input.PaymentMethod == null || p.PaymentMethod == input.PaymentMethod) &&
                (!input.IsSuccessful.HasValue || p.IsSuccessful == input.IsSuccessful.Value) &&
                (!input.FromDate.HasValue || p.PaymentDate >= input.FromDate.Value) &&
                (!input.ToDate.HasValue || p.PaymentDate <= input.ToDate.Value));

            var paginated = _utility.Paginate(allPayments, input.PageNumber, input.PageSize);
            var response = _mapper.Map<List<PaymentResponseDTO>>(paginated);

            return PageBaseResponse<List<PaymentResponseDTO>>.Success(response, ResponseMessages.PaymentsRetrieved,
                count: response.Count, totalPageCount: allPayments.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
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
            if (trigger == PaymentTrigger.Complete)
                payment.IsSuccessful = true;
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
