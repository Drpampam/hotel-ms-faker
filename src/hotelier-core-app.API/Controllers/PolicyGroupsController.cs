using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    /// <summary>
    /// Controller for managing policy groups, permissions, users, and policies.
    /// </summary>
    public class PolicyGroupsController : ControllerBase
    {
        private readonly IPolicyGroupService _policyGroupService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyGroupsController"/> class.
        /// </summary>
        /// <param name="policyGroupService">Service for policy group operations.</param>
        /// <param name="tokenHelper">Service for token operations.</param>
        /// <param name="accessor">HTTP context accessor.</param>
        public PolicyGroupsController(IPolicyGroupService policyGroupService,
            ITokenService tokenHelper,
            IHttpContextAccessor accessor)
        {
            _policyGroupService = policyGroupService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpGet("permissions")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<List<PermissionDTO>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Gets all permissions available in the system.
        /// </summary>
        /// <returns>A list of all permissions.</returns>
        public async Task<IActionResult> GetAllPermission()
        {
            BaseResponse<List<PermissionDTO>> response = await _policyGroupService.GetAllPermission();
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Adds a new policy group.
        /// </summary>
        /// <param name="request">The policy group creation request.</param>
        /// <returns>The result of the add operation.</returns>
        public async Task<IActionResult> AddPolicyGroup(AddPolicyGroupDTO request)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.ActivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _policyGroupService.AddPolicyGroup(request, auditLog);
            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Gets policy groups based on the specified request.
        /// </summary>
        /// <param name="request">The request for policy groups.</param>
        /// <returns>The result containing policy groups.</returns>
        public async Task<IActionResult> GetPolicyGroups(GetPolicyGroupsRequestDTO request)
        {
            BaseResponse response = await _policyGroupService.GetPolicyGroups(request);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Gets a single policy group by ID.
        /// </summary>
        /// <param name="id">The ID of the policy group.</param>
        /// <returns>The result containing the policy group.</returns>
        public async Task<IActionResult> GetSinglePolicyGroup(long id)
        {
            BaseResponse response = await _policyGroupService.GetSinglePolicyGroup(id);
            return Ok(response);
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Updates an existing policy group.
        /// </summary>
        /// <param name="request">The update request for the policy group.</param>
        /// <returns>The result of the update operation.</returns>
        public async Task<IActionResult> UpdatePolicyGroup(UpdatePolicyGroupDTO request)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.ActivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _policyGroupService.UpdatePolicyGroup(request, auditLog);
            return Ok(response);
        }

        [HttpPost("users")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Adds a user to a policy group.
        /// </summary>
        /// <param name="request">The request to add a user to a policy group.</param>
        /// <returns>The result of the add operation.</returns>
        public async Task<IActionResult> AddUserToPolicyGroup(AddUserToPolicyGroupDTO request)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.ActivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _policyGroupService.AddUserToPolicyGroup(request, auditLog);
            return Ok(response);
        }

        [HttpDelete("{policyGroupId}/users/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Removes a user from a policy group.
        /// </summary>
        /// <param name="userId">The ID of the user to remove.</param>
        /// <param name="policyGroupId">The ID of the policy group.</param>
        /// <returns>The result of the remove operation.</returns>
        public async Task<IActionResult> RemoveUserFromPolicyGroup(long userId, long policyGroupId)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.ActivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _policyGroupService.RemoveUserFromPolicyGroup(userId, policyGroupId, auditLog);
            return Ok(response);
        }

        [HttpPost("policies")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Adds a policy to a policy group.
        /// </summary>
        /// <param name="request">The request to add a policy to a policy group.</param>
        /// <returns>The result of the add operation.</returns>
        public async Task<IActionResult> AddPolicyToPolicyGroup(AddPolicyToPolicyGroupDTO request)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.ActivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _policyGroupService.AddPolicyToPolicyGroup(request, auditLog);
            return Ok(response);
        }

        [HttpDelete("{policyGroupId}/policies/{policyId}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        /// <summary>
        /// Removes a policy from a policy group.
        /// </summary>
        /// <param name="policyGroupId">The ID of the policy group.</param>
        /// <param name="policyId">The ID of the policy to remove.</param>
        /// <returns>The result of the remove operation.</returns>
        public async Task<IActionResult> RemovePolicyFromPolicyGroup(long policyGroupId, long policyId)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.ActivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _policyGroupService.RemovePolicyFromPolicyGroup(policyGroupId, policyId, auditLog);
            return Ok(response);
        }
    }
}
