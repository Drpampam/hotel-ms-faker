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
using Microsoft.AspNetCore.Identity;

namespace hotelier_core_app.Service.Implementation
{
    /// <summary>
    /// Provides business logic for managing service requests and their state transitions.
    /// </summary>
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IDBCommandRepository<ServiceRequest> _serviceRequestCommandRepository;
        private readonly IDBQueryRepository<ServiceRequest> _serviceRequestQueryRepository;
        private readonly IDBQueryRepository<Reservation> _reservationQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;

        public ServiceRequestService(
            IDBCommandRepository<ServiceRequest> serviceRequestCommandRepository,
            IDBQueryRepository<ServiceRequest> serviceRequestQueryRepository,
            IDBQueryRepository<Reservation> reservationQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUtility utility)
        {
            _serviceRequestCommandRepository = serviceRequestCommandRepository;
            _serviceRequestQueryRepository = serviceRequestQueryRepository;
            _reservationQueryRepository = reservationQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _notificationService = notificationService;
            _userManager = userManager;
            _mapper = mapper;
            _utility = utility;
        }

        public async Task<BaseResponse<ServiceRequestResponseDTO>> CreateServiceRequestAsync(CreateServiceRequestDTO request, AuditLog auditLog)
        {
            var reservation = await _reservationQueryRepository.FindAsync(request.ReservationId);
            if (reservation == null)
                return BaseResponse<ServiceRequestResponseDTO>.Failure(new ServiceRequestResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            var serviceRequest = _mapper.Map<ServiceRequest>(request);
            serviceRequest.ServiceRequestState = ServiceRequestState.Requested;
            serviceRequest.Status = ServiceRequestState.Requested.ToString();
            serviceRequest.CreatedBy = auditLog.PerformedBy;
            serviceRequest.CreationDate = DateTime.UtcNow;

            _serviceRequestCommandRepository.Add(serviceRequest);
            _auditLogCommandRepository.Add(auditLog);
            await _serviceRequestCommandRepository.SaveAsync();

            var response = _mapper.Map<ServiceRequestResponseDTO>(serviceRequest);
            return BaseResponse<ServiceRequestResponseDTO>.Success(response, ResponseMessages.ServiceRequestCreated, ResponseStatusCode.ServiceRequestCreated);
        }

        public async Task<BaseResponse<ServiceRequestResponseDTO>> GetServiceRequestByIdAsync(long serviceRequestId)
        {
            var serviceRequest = await _serviceRequestQueryRepository.FindAsync(serviceRequestId);
            if (serviceRequest == null)
                return BaseResponse<ServiceRequestResponseDTO>.Failure(new ServiceRequestResponseDTO(), ResponseMessages.ServiceRequestNotFound, ResponseStatusCode.ServiceRequestNotFound);

            var response = _mapper.Map<ServiceRequestResponseDTO>(serviceRequest);
            return BaseResponse<ServiceRequestResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<ServiceRequestResponseDTO>>> GetServiceRequestsAsync(GetServiceRequestsInputDTO input)
        {
            var all = await _serviceRequestQueryRepository.GetByAsync(sr =>
                !sr.IsDeleted &&
                (!input.ReservationId.HasValue || sr.ReservationId == input.ReservationId.Value) &&
                (input.ServiceType == null || sr.ServiceType == input.ServiceType));

            var paginated = _utility.Paginate(all, input.PageNumber, input.PageSize);
            var response = _mapper.Map<List<ServiceRequestResponseDTO>>(paginated);

            return PageBaseResponse<List<ServiceRequestResponseDTO>>.Success(response, ResponseMessages.ServiceRequestsRetrieved,
                count: response.Count, totalPageCount: all.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }

        /// <summary>
        /// Changes the state of a service request using the specified trigger.
        /// </summary>
        /// <param name="serviceRequestId">The ID of the service request.</param>
        /// <param name="trigger">The trigger to fire for the state change.</param>
        /// <returns>Returns the updated service request state, or failure if not found or trigger is invalid.</returns>
        public async Task<BaseResponse<ServiceRequestStateResponseDTO>> ChangeServiceRequestStateAsync(long serviceRequestId, ServiceRequestTrigger trigger)
        {
            var serviceRequest = await _serviceRequestQueryRepository.FindAsync(serviceRequestId);
            if (serviceRequest == null)
                return BaseResponse<ServiceRequestStateResponseDTO>.Failure(new ServiceRequestStateResponseDTO(), "ServiceRequest not found", ResponseStatusCode.NoRecordFound);
            serviceRequest.ConfigureStateMachine();
            if (serviceRequest.StateMachine == null || !serviceRequest.StateMachine.CanFire(trigger))
                return BaseResponse<ServiceRequestStateResponseDTO>.Failure(new ServiceRequestStateResponseDTO(), "Invalid trigger", ResponseStatusCode.InvalidData);
            serviceRequest.StateMachine.Fire(trigger);
            await _serviceRequestCommandRepository.UpdateAsync(serviceRequest);

            // Notify guest when service request is completed
            if (trigger == ServiceRequestTrigger.Complete)
            {
                var reservation = await _reservationQueryRepository.FindAsync(serviceRequest.ReservationId);
                if (reservation != null)
                {
                    var guest = await _userManager.FindByIdAsync(reservation.GuestId.ToString());
                    if (!string.IsNullOrEmpty(guest?.Email))
                        _ = _notificationService.SendServiceRequestCompletedAsync(serviceRequest, guest.Email, guest.FullName ?? guest.Email);
                }
            }

            var stateMachine = serviceRequest.StateMachine;
            var triggers = stateMachine != null ? (await stateMachine.PermittedTriggersAsync).ToList() : new List<ServiceRequestTrigger>();
            var responseDto = new ServiceRequestStateResponseDTO
            {
                ServiceRequestId = serviceRequest.Id,
                State = stateMachine != null ? stateMachine.State : default,
                AvailableTriggers = triggers.ToList()
            };
            return BaseResponse<ServiceRequestStateResponseDTO>.Success(responseDto, "State changed successfully", ResponseStatusCode.OperationSuccessful);
        }

        /// <summary>
        /// Retrieves the current state and available triggers for a service request.
        /// </summary>
        /// <param name="serviceRequestId">The ID of the service request.</param>
        /// <returns>Returns the service request state and available triggers, or failure if not found.</returns>
        public async Task<BaseResponse<ServiceRequestStateResponseDTO>> GetServiceRequestStateAsync(long serviceRequestId)
        {
            var serviceRequest = await _serviceRequestQueryRepository.FindAsync(serviceRequestId);
            if (serviceRequest == null)
                return BaseResponse<ServiceRequestStateResponseDTO>.Failure(new ServiceRequestStateResponseDTO(), "ServiceRequest not found", ResponseStatusCode.NoRecordFound);
            serviceRequest.ConfigureStateMachine();
            var stateMachine = serviceRequest.StateMachine;
            var triggers = stateMachine != null ? (await stateMachine.PermittedTriggersAsync).ToList() : new List<ServiceRequestTrigger>();
            var responseDto = new ServiceRequestStateResponseDTO
            {
                ServiceRequestId = serviceRequest.Id,
                State = stateMachine != null ? stateMachine.State : default,
                AvailableTriggers = triggers.ToList()
            };
            return BaseResponse<ServiceRequestStateResponseDTO>.Success(responseDto, "State fetched successfully", ResponseStatusCode.OperationSuccessful);
        }

        /// <summary>
        /// Retrieves all available triggers for a service request's current state.
        /// </summary>
        /// <param name="serviceRequestId">The ID of the service request.</param>
        /// <returns>Returns a list of available triggers, or failure if service request not found.</returns>
        public async Task<BaseResponse<List<ServiceRequestTrigger>>> GetAvailableTriggersAsync(long serviceRequestId)
        {
            var serviceRequest = await _serviceRequestQueryRepository.FindAsync(serviceRequestId);
            if (serviceRequest == null)
                return BaseResponse<List<ServiceRequestTrigger>>.Failure(new List<ServiceRequestTrigger>(), "ServiceRequest not found", ResponseStatusCode.NoRecordFound);
            serviceRequest.ConfigureStateMachine();
            var stateMachine = serviceRequest.StateMachine;
            var triggers = stateMachine != null ? (await stateMachine.PermittedTriggersAsync).ToList() : new List<ServiceRequestTrigger>();
            return BaseResponse<List<ServiceRequestTrigger>>.Success(triggers.ToList(), "Available triggers fetched successfully", ResponseStatusCode.OperationSuccessful);
        }
    }
}
