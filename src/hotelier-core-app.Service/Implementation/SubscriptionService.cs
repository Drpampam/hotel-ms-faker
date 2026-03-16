using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation;

/// <summary>
/// Provides business logic for managing subscription plans and tenant assignments.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IDBCommandRepository<SubscriptionPlan> _planCommandRepository;
    private readonly IDBQueryRepository<SubscriptionPlan> _planQueryRepository;
    private readonly IDBQueryRepository<Tenant> _tenantQueryRepository;
    private readonly IDBCommandRepository<Tenant> _tenantCommandRepository;
    private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
    private readonly IMapper _mapper;

    public SubscriptionService(IDBCommandRepository<SubscriptionPlan> planCommandRepository,
        IDBQueryRepository<SubscriptionPlan> planQueryRepository,
        IDBQueryRepository<Tenant> tenantQueryRepository,
        IDBCommandRepository<Tenant> tenantCommandRepository,
        IDBCommandRepository<AuditLog> auditLogCommandRepository,
        IMapper mapper)
    {
        this._planCommandRepository = planCommandRepository;
        this._planQueryRepository = planQueryRepository;
        this._tenantQueryRepository = tenantQueryRepository;
        this._tenantCommandRepository = tenantCommandRepository;
        this._auditLogCommandRepository = auditLogCommandRepository;
        this._mapper = mapper;
    }
    /// <summary>
    /// Creates a new subscription plan if it does not already exist.
    /// </summary>
    /// <param name="request">The subscription plan creation details.</param>
    /// <param name="auditLog">Audit log information for the operation.</param>
    /// <returns>Returns a success response if created, otherwise failure if the plan exists.</returns>
    public async Task<BaseResponse> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDTO request, AuditLog auditLog)
    {
        var existingPlan = await _planQueryRepository.GetByDefaultAsync(p => p.Name == request.Name && p.IsDeleted == false);
        if (existingPlan != null)
            return BaseResponse.Failure($"{ResponseMessages.SubscriptionExist}': ' {request.Name}");

        var plan = _mapper.Map<SubscriptionPlan>(request);
        plan.CreationDate = DateTime.UtcNow;
        plan.CreatedBy = auditLog.PerformedBy;
        await _planCommandRepository.AddAsync(plan);
        await _planCommandRepository.SaveAsync();
        await _auditLogCommandRepository.AddAsync(auditLog);
        await _auditLogCommandRepository.SaveAsync();
        return BaseResponse.Success(ResponseMessages.SubscriptionCreated);
    }

    /// <summary>
    /// Retrieves a subscription plan by its ID.
    /// </summary>
    /// <param name="id">The ID of the subscription plan.</param>
    /// <returns>Returns the subscription plan DTO if found, otherwise failure.</returns>
    public async Task<BaseResponse<SubscriptionPlanResponseDTO>> GetSubscriptionPlanByIdAsync(long id)
    {
        var plan = await _planQueryRepository.FindAsync(id);
        if (plan == null)
            return BaseResponse<SubscriptionPlanResponseDTO>.Failure(null, ResponseMessages.SubscriptionNotExist);

        var response = _mapper.Map<SubscriptionPlanResponseDTO>(plan);
        return BaseResponse<SubscriptionPlanResponseDTO>.Success(response);
    }

    /// <summary>
    /// Retrieves all subscription plans in the system.
    /// </summary>
    /// <returns>Returns a list of all subscription plan DTOs.</returns>
    public async Task<BaseResponse<List<SubscriptionPlanResponseDTO>>> GetAllSubscriptionPlansAsync()
    {
        var plans = await _planQueryRepository.GetAllAsync();
        var response = _mapper.Map<List<SubscriptionPlanResponseDTO>>(plans);
        return BaseResponse<List<SubscriptionPlanResponseDTO>>.Success(response);
    }

    /// <summary>
    /// Marks a subscription plan as deleted by its ID.
    /// </summary>
    /// <param name="id">The ID of the subscription plan to delete.</param>
    /// <param name="auditLog">Audit log information for the operation.</param>
    /// <returns>Returns a success response if deleted, otherwise failure if not found.</returns>
    public async Task<BaseResponse> DeleteSubscriptionPlanAsync(long id, AuditLog auditLog)
    {
        var plan = await _planQueryRepository.FindAsync(id);
        if (plan == null)
            return BaseResponse.Failure("Subscription plan not found.");

        plan.IsDeleted = true;
        plan.LastModifiedDate = DateTime.UtcNow;
        plan.ModifiedBy = auditLog.PerformedBy;
        await _planCommandRepository.UpdateAsync(plan);
        await _auditLogCommandRepository.AddAsync(auditLog);
        await _auditLogCommandRepository.SaveAsync();

        return BaseResponse.Success("Subscription plan deleted successfully.");
    }
    /// <summary>
    /// Assigns a subscription plan to a tenant for a specified number of months.
    /// </summary>
    /// <param name="request">The assignment details including tenant and plan information.</param>
    /// <param name="auditLog">Audit log information for the operation.</param>
    /// <returns>Returns a success response if assigned, otherwise failure if tenant or plan not found or months invalid.</returns>
    public async Task<BaseResponse> AssignSubscriptionPlanToTenantAsync(AssignSubscriptionPlanDTO request, AuditLog auditLog)
    {
        var tenant = await _tenantQueryRepository.FindAsync(request.TenantId);
        if (tenant == null)
            return BaseResponse.Failure($"Tenant with ID {request.TenantId} not found.");

        var plan = await _planQueryRepository.GetByDefaultAsync(p => p.Name == request.SubscriptionPlan.ToString());
        if (plan == null)
            return BaseResponse.Failure($"Subscription plan '{request.SubscriptionPlan}' not found in the database.");

        if (request.NumberOfMonths <= 0)
            return BaseResponse.Failure("Number of months must be greater than 0.");

        tenant.SubscriptionPlanId = plan.Id;
        tenant.SubscriptionStartDate = DateTime.UtcNow;
        tenant.SubscriptionEndDate = DateTime.UtcNow.AddMonths(request.NumberOfMonths);

        await _tenantCommandRepository.UpdateAsync(tenant);
        await _auditLogCommandRepository.AddAsync(auditLog);
        await _auditLogCommandRepository.SaveAsync();

        return BaseResponse.Success($"Subscription plan '{request.SubscriptionPlan}' assigned to tenant for {request.NumberOfMonths} months successfully.");
    }

}