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
    /// Provides business logic for managing service requests and their state transitions.
    /// </summary>
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IDBCommandRepository<ServiceRequest> _serviceRequestCommandRepository;
        private readonly IDBQueryRepository<ServiceRequest> _serviceRequestQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IMapper _mapper;

        public ServiceRequestService(
            IDBCommandRepository<ServiceRequest> serviceRequestCommandRepository,
            IDBQueryRepository<ServiceRequest> serviceRequestQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IMapper mapper)
        {
            _serviceRequestCommandRepository = serviceRequestCommandRepository;
            _serviceRequestQueryRepository = serviceRequestQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _mapper = mapper;
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
