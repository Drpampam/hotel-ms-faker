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
/// Provides business logic for managing roles and their policy group assignments.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IDBCommandRepository<ApplicationRole> _roleCommandRepository;
    private readonly IDBQueryRepository<ApplicationRole> _roleQueryRepository;
    private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
    private readonly IDBCommandRepository<RolePolicyGroup> _rolePolicyGroupCommandRepository;
    private readonly IDBQueryRepository<RolePolicyGroup> _rolePolicyGroupQueryRepository;
    private readonly IMapper _mapper;

    public RoleService(
        IDBCommandRepository<ApplicationRole> roleCommandRepository,
        IDBQueryRepository<ApplicationRole> roleQueryRepository,
        IDBCommandRepository<AuditLog> auditLogCommandRepository,
        IDBCommandRepository<RolePolicyGroup> rolePolicyGroupCommandRepository,
        IDBQueryRepository<RolePolicyGroup> rolePolicyGroupQueryRepository,
        IMapper mapper)
    {
        this._roleCommandRepository = roleCommandRepository;
        this._roleQueryRepository = roleQueryRepository;
        this._auditLogCommandRepository = auditLogCommandRepository;
        this._rolePolicyGroupCommandRepository = rolePolicyGroupCommandRepository;
        this._rolePolicyGroupQueryRepository = rolePolicyGroupQueryRepository;
        this._mapper = mapper;
    }
    /// <summary>
    /// Creates a new role and assigns policy groups if provided.
    /// </summary>
    /// <param name="request">The role creation details.</param>
    /// <param name="auditLog">Audit log information for the operation.</param>
    /// <returns>Returns a success response if created, otherwise failure if the role exists.</returns>
    public async Task<BaseResponse> CreateRoleAsync(CreateRoleRequestDTO request, AuditLog auditLog)
    {
        if (!request.TenantId.HasValue)
        {
            return BaseResponse.Failure("TenantId is required.", ResponseStatusCode.InvalidData);
        }

        var existingRole = await _roleQueryRepository.GetByDefaultAsync(r => r.Name == request.RoleName && r.IsDeleted == false && r.TenantId == request.TenantId);
        if (existingRole != null) return BaseResponse.Failure(ResponseMessages.RoleExist);

        var role = _mapper.Map<ApplicationRole>(request);
        role.Name = request.RoleName;
        role.CreationDate = DateTime.UtcNow;
        role.CreatedBy = auditLog.PerformedBy;
        role.TenantId = request.TenantId;
        await _roleCommandRepository.AddAsync(role);
        await _roleCommandRepository.SaveAsync();

        // Assign PolicyGroups to Role via RolePolicyGroup
        if (request.PolicyGroupIds != null)
        {
            foreach (var policyGroupId in request.PolicyGroupIds)
            {
                var rolePolicyGroup = new RolePolicyGroup
                {
                    RoleId = role.Id,
                    PolicyGroupId = policyGroupId,
                    TenantId = request.TenantId.Value
                };
                await _rolePolicyGroupCommandRepository.AddAsync(rolePolicyGroup);
            }
            await _rolePolicyGroupCommandRepository.SaveAsync();
        }

        await _auditLogCommandRepository.AddAsync(auditLog);
        await _auditLogCommandRepository.SaveAsync();

        return BaseResponse.Success("Role created successfully.");
    }

    /// <summary>
    /// Updates an existing role's details.
    /// </summary>
    /// <param name="request">The updated role details.</param>
    /// <param name="auditLog">Audit log information for the operation.</param>
    /// <returns>Returns a success response if updated, otherwise failure if not found.</returns>
    public async Task<BaseResponse> UpdateRoleAsync(UpdateRoleRequestDTO request, AuditLog auditLog)
    {
        var role = await _roleQueryRepository.FindAsync(request.Id);
        if (role == null) return BaseResponse.Failure(ResponseMessages.RoleNotExist);

        role.Name = request.RoleName;
        role.LastModifiedDate = DateTime.UtcNow;
        role.ModifiedBy = auditLog.PerformedBy;
        await _roleCommandRepository.UpdateAsync(role);
        await _auditLogCommandRepository.AddAsync(auditLog);
        await _auditLogCommandRepository.SaveAsync();

        return BaseResponse.Success("Role updated successfully.");
    }

    /// <summary>
    /// Retrieves a role by its ID.
    /// </summary>
    /// <param name="id">The ID of the role.</param>
    /// <returns>Returns the role DTO if found, otherwise failure.</returns>
    public async Task<BaseResponse<RoleResponseDTO>> GetRoleByIdAsync(long id)
    {
        var role = await _roleQueryRepository.FindAsync(id);
        if (role == null) return BaseResponse<RoleResponseDTO>.Failure(new RoleResponseDTO(), ResponseMessages.RoleNotExist);

        var response = _mapper.Map<RoleResponseDTO>(role);
        return BaseResponse<RoleResponseDTO>.Success(response);
    }

    /// <summary>
    /// Retrieves all roles in the system.
    /// </summary>
    /// <returns>Returns a list of all role DTOs.</returns>
    public async Task<BaseResponse<List<RoleResponseDTO>>> GetAllRolesAsync()
    {
        var roles = await _roleQueryRepository.GetAllAsync();
        var response = _mapper.Map<List<RoleResponseDTO>>(roles);
        return BaseResponse<List<RoleResponseDTO>>.Success(response);
    }

    /// <summary>
    /// Marks a role as deleted by its ID.
    /// </summary>
    /// <param name="id">The ID of the role to delete.</param>
    /// <param name="auditLog">Audit log information for the operation.</param>
    /// <returns>Returns a success response if deleted, otherwise failure if not found.</returns>
    public async Task<BaseResponse> DeleteRoleAsync(long id, AuditLog auditLog)
    {
        var role = await _roleQueryRepository.FindAsync(id);
        if (role == null) return BaseResponse.Failure(ResponseMessages.RoleNotExist);

        role.IsDeleted = true;
        role.LastModifiedDate = DateTime.UtcNow;
        await _roleCommandRepository.UpdateAsync(role);
        await _auditLogCommandRepository.AddAsync(auditLog);
        await _auditLogCommandRepository.SaveAsync();

        return BaseResponse.Success("Role removed successfully.");
    }
}
