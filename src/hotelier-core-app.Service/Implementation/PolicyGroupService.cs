using AutoMapper;
using hotelier_core_app.Core.Constants;
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
    /// Provides business logic for managing policy groups, permissions, and user assignments.
    /// </summary>
    public class PolicyGroupService : IPolicyGroupService
    {
        private readonly IDBCommandRepository<PolicyGroup> _policyGroupCommandRepository;
        private readonly IDBQueryRepository<PolicyGroup> _policyGroupQueryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IDBCommandRepository<ApplicationUserPolicyGroup> _userPolicyCommandRepository;
        private readonly IDBQueryRepository<ApplicationUserPolicyGroup> _userPolicyQueryRepository;
        private readonly IDBQueryRepository<ModuleGroup> _moduleGroupQueryRepository;
        private readonly IDBQueryRepository<Permission> _permissionQueryRepository;
        private readonly IDBQueryRepository<PolicyModulePermission> _pmpQueryRepository;
        private readonly IDBCommandRepository<PolicyModulePermission> _pmpCommandRepository;
        private readonly IMapper _mapper;

        public PolicyGroupService(IDBCommandRepository<PolicyGroup> policyGroupCommandRepository,
            /// <summary>
            /// Retrieves all permissions available in the system.
            /// </summary>
            /// <returns>Returns a list of permission DTOs.</returns>
            IDBQueryRepository<PolicyGroup> policyGroupQueryRepository,
            UserManager<ApplicationUser> userManager,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IDBCommandRepository<ApplicationUserPolicyGroup> userPolicyCommandRepository,
            IDBQueryRepository<ApplicationUserPolicyGroup> userPolicyQueryRepository,
            IDBQueryRepository<ModuleGroup> moduleGroupQueryRepository,
            IDBQueryRepository<Permission> permissionQueryRepository,
            IDBQueryRepository<PolicyModulePermission> pmpQueryRepository,
            IDBCommandRepository<PolicyModulePermission> pmpCommandRepository,
            IMapper mapper)
        {
            _policyGroupCommandRepository = policyGroupCommandRepository;
            _policyGroupQueryRepository = policyGroupQueryRepository;
            _userManager = userManager;
            _auditLogCommandRepository = auditLogCommandRepository;
            _userPolicyCommandRepository = userPolicyCommandRepository;
            _userPolicyQueryRepository = userPolicyQueryRepository;
            _moduleGroupQueryRepository = moduleGroupQueryRepository;
            _permissionQueryRepository = permissionQueryRepository;
            _pmpQueryRepository = pmpQueryRepository;
            _pmpCommandRepository = pmpCommandRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<PermissionDTO>>> GetAllPermission()
        /// <summary>
        /// Asynchronously retrieves all permissions from the data source, maps them to <see cref="PermissionDTO"/> objects,
        /// and returns a successful <see cref="BaseResponse{T}"/> containing the list of permissions.
        /// </summary>
        /// <returns> A containing the list of permissions, a success message, and a success status code.</returns>
        {
            try
            {
                var permissions = await _permissionQueryRepository.GetAllAsync();
                var permissionsDTO = _mapper.Map<List<PermissionDTO>>(permissions) ?? new List<PermissionDTO>();

                return BaseResponse<List<PermissionDTO>>.Success(
                    permissionsDTO,
                    ResponseMessages.OperationSuccessful,
                    ResponseStatusCode.OperationSuccessful
                );
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<PermissionDTO>>
                {
                    Status = false,
                    Data = new List<PermissionDTO>(),
                    Message = $"An error occurred while retrieving permissions: {ex.Message}",
                    StatusCode = ResponseStatusCode.OperationFailed
                };
            }
        }

        public async Task<BaseResponse> AddPolicyGroup(AddPolicyGroupDTO request, AuditLog auditLog)
        /// <summary>
        /// Adds a new policy group for a tenant if it does not already exist.
        /// </summary>
        /// <param name="request">The policy group creation details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if created, otherwise failure if the group exists or user is invalid.</returns>
        {
            if (request == null || auditLog == null || string.IsNullOrEmpty(auditLog.PerformerEmail))
            {
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
            }

            var policyGroup = _policyGroupQueryRepository.GetByDefault(p => p.Name == request.Name && p.TenantId == request.TenantId);
            if (policyGroup != null)
            {
                return BaseResponse.Failure(ResponseMessages.PolicyGroupExists, ResponseStatusCode.PolicyGroupExists);
            }

            var currentUser = await _userManager.FindByEmailAsync(auditLog.PerformerEmail);
            if (currentUser == null)
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);

            policyGroup = new PolicyGroup
            {
                Name = request.Name,
                Description = request.Description,
                TenantId = request.TenantId,
                CreatedBy = auditLog.PerformedBy,
                CreationDate = DateTime.UtcNow
            };

            _auditLogCommandRepository.Add(auditLog);
            _policyGroupCommandRepository.Add(policyGroup);

            if (await _policyGroupCommandRepository.SaveAsync() == 0)
            {
                return BaseResponse.Failure("Policy group creation failed.", ResponseStatusCode.OperationFailed);
            }

            return BaseResponse.Success(ResponseMessages.OperationSuccessful,
                ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse> UpdatePolicyGroup(UpdatePolicyGroupDTO request, AuditLog auditLog)
        /// <summary>
        /// Updates an existing policy group's details.
        /// </summary>
        /// <param name="request">The updated policy group details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if updated, otherwise failure if not found.</returns>
        {
            PolicyGroup? policyGroup = await _policyGroupQueryRepository.FindAsync(request.Id);
            if (policyGroup == null) return BaseResponse.Failure(ResponseMessages.PolicyGroupDoesNotExist, ResponseStatusCode.PolicyGroupDoesNotExist);

            policyGroup.Name = request.Name;
            policyGroup.Description = request.Description;
            policyGroup.TenantId = request.TenantId;
            policyGroup.ModifiedBy = auditLog.PerformerEmail;
            policyGroup.LastModifiedDate = DateTime.UtcNow;

            _auditLogCommandRepository.Add(auditLog);
            _policyGroupCommandRepository.Update(policyGroup);
            _policyGroupCommandRepository.Save();

            return BaseResponse.Success(ResponseMessages.OperationSuccessful,
                ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse> AddUserToPolicyGroup(AddUserToPolicyGroupDTO request, AuditLog auditLog)
        /// <summary>
        /// Adds a user to a policy group.
        /// </summary>
        /// <param name="request">The user and policy group assignment details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if added, otherwise failure if not found.</returns>
        {
            PolicyGroup policyGroup = await _policyGroupQueryRepository.FindAsync(request.PolicyGroupId);
            if (policyGroup == null) return BaseResponse.Failure(ResponseMessages.PolicyGroupDoesNotExist, ResponseStatusCode.PolicyGroupDoesNotExist);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null) return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);

            var userPolicy = new ApplicationUserPolicyGroup();
            userPolicy.UserId = request.UserId;
            userPolicy.PolicyGroupId = request.PolicyGroupId;
            userPolicy.CreatedBy = auditLog.PerformerEmail;
            userPolicy.CreationDate = DateTime.UtcNow;

            _auditLogCommandRepository.Add(auditLog);
            _userPolicyCommandRepository.Add(userPolicy);
            await _userPolicyCommandRepository.SaveAsync();

            return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse> RemoveUserFromPolicyGroup(long userId, long policyGroupId, AuditLog auditLog)
        /// <summary>
        /// Removes a user from a policy group.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="policyGroupId">The ID of the policy group.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if removed, otherwise failure if not found.</returns>
        {
            var userPolicy = await _userPolicyQueryRepository.GetByDefaultAsync(u => u.UserId == userId && u.PolicyGroupId == policyGroupId);
            if (userPolicy == null) return BaseResponse.Failure(ResponseMessages.UserNotInPolicyGroup, ResponseStatusCode.UserNotInPolicyGroup);

            _userPolicyCommandRepository.Delete(userPolicy);
            _auditLogCommandRepository.Add(auditLog);
            _auditLogCommandRepository.Save();

            return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse> AddPolicyToPolicyGroup(AddPolicyToPolicyGroupDTO request, AuditLog auditLog)
        /// <summary>
        /// Adds a permission to a policy group for a specific module group.
        /// </summary>
        /// <param name="request">The permission and policy group assignment details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if added, otherwise failure if not found.</returns>
        {
            PolicyGroup policyGroup = await _policyGroupQueryRepository.FindAsync(request.PolicyGroupId);
            if (policyGroup == null) return BaseResponse.Failure(ResponseMessages.PolicyGroupDoesNotExist, ResponseStatusCode.PolicyGroupDoesNotExist);

            var permission = await _permissionQueryRepository.FindAsync(request.PermissionId);
            if (permission == null) return BaseResponse.Failure(ResponseMessages.PermissionDoesNotExist, ResponseStatusCode.PermissionDoesNotExist);

            var moduleGroup = await _moduleGroupQueryRepository.FindAsync(request.ModuleGroupId);
            if (moduleGroup == null) return BaseResponse.Failure(ResponseMessages.ModuleGroupNotExist, ResponseStatusCode.ModuleGroupNotExist);

            var pmp = new PolicyModulePermission();
            pmp.PermissionId = request.PermissionId;
            pmp.PolicyGroupId = request.PolicyGroupId;
            pmp.ModuleGroupId = request.ModuleGroupId;
            pmp.CreatedBy = auditLog.PerformerEmail;
            pmp.CreationDate = DateTime.UtcNow;

            _auditLogCommandRepository.Add(auditLog);
            _pmpCommandRepository.Add(pmp);
            _pmpCommandRepository.Save();

            return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse> RemovePolicyFromPolicyGroup(long policyGroupId, long policy, AuditLog auditLog)
        /// <summary>
        /// Removes a permission from a policy group.
        /// </summary>
        /// <param name="policyGroupId">The ID of the policy group.</param>
        /// <param name="policy">The ID of the permission to remove.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if removed, otherwise failure if not found.</returns>
        {
            var pmp = await _pmpQueryRepository.GetByDefaultAsync(p => p.Id == policy && p.PolicyGroupId == policyGroupId);
            if (pmp == null) return BaseResponse.Failure(ResponseMessages.PolicyDoesNotExist, ResponseStatusCode.PolicyDoesNotExist);

            _auditLogCommandRepository.Add(auditLog);
            _pmpCommandRepository.Delete(pmp);
            _pmpCommandRepository.Save();

            return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<List<GetPolicyGroupResponseDTO>>> GetPolicyGroups(GetPolicyGroupsRequestDTO request)
        /// <summary>
        /// Retrieves all policy groups for a tenant, including their module permissions.
        /// </summary>
        /// <param name="request">The request containing tenant information.</param>
        /// <returns>Returns a list of policy group DTOs.</returns>
        {
            var policyGroups = _policyGroupQueryRepository.GetAllIncluding(p => p.ModulePermissions).Where(p => p.TenantId == request.TenantId);
            var response = _mapper.Map<List<GetPolicyGroupResponseDTO>>(policyGroups.ToList());
            return BaseResponse<List<GetPolicyGroupResponseDTO>>.Success(response, ResponseMessages.OperationSuccessful,
                ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<GetPolicyGroupResponseDTO>> GetSinglePolicyGroup(long id)
        /// <summary>
        /// Retrieves a single policy group by its ID.
        /// </summary>
        /// <param name="id">The ID of the policy group.</param>
        /// <returns>Returns the policy group DTO if found, otherwise failure.</returns>
        {
            PolicyGroup policyGroup = await _policyGroupQueryRepository.FindAsync(id);
            if (policyGroup == null) return BaseResponse<GetPolicyGroupResponseDTO>.Failure(null, ResponseMessages.PolicyGroupDoesNotExist, ResponseStatusCode.PolicyGroupDoesNotExist);
            return BaseResponse<GetPolicyGroupResponseDTO>.Success(_mapper.Map<GetPolicyGroupResponseDTO>(policyGroup), ResponseMessages.OperationSuccessful,
                ResponseStatusCode.OperationSuccessful);
        }
    }
}
