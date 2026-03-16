using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers;

/// <summary>
/// Controller for managing user roles and related operations.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleController"/> class.
    /// </summary>
    /// <param name="roleService">Service for role operations.</param>
    /// <param name="tokenService">Service for token operations.</param>
    /// <param name="accessor">HTTP context accessor.</param>
    public RoleController(IRoleService roleService, ITokenService tokenService, IHttpContextAccessor accessor)
    {
        this._roleService = roleService;
        this._tokenService = tokenService;
        this._accessor = accessor;
    }

    [HttpPost("create-role")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
    /// <summary>
    /// Creates a new user role.
    /// </summary>
    /// <param name="request">The role creation request.</param>
    /// <returns>The result of the creation operation.</returns>
    public async Task<IActionResult> CreateRole(CreateRoleRequestDTO request)
    {
        AuditLog auditLog = new AuditLog
        {
            Action = UserAction.CreateUserRole,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenService.GetUserEmail(Request),
            PerformedAgainst = request.RoleName,
            MacAddress = _tokenService.GetMacAddress(Request)
        };

        var response = await _roleService.CreateRoleAsync(request, auditLog);
        return Ok(response);
    }

    [HttpPut("update-role")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
    /// <summary>
    /// Updates an existing user role.
    /// </summary>
    /// <param name="request">The role update request.</param>
    /// <returns>The result of the update operation.</returns>
    public async Task<IActionResult> UpdateRole(UpdateRoleRequestDTO request)
    {
        AuditLog auditLog = new AuditLog
        {
            Action = UserAction.EditUserRole,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenService.GetUserEmail(Request),
            PerformedAgainst = request.RoleName,
            MacAddress = _tokenService.GetMacAddress(Request)
        };

        var response = await _roleService.UpdateRoleAsync(request, auditLog);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<RoleResponseDTO>))]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
    /// <summary>
    /// Gets a user role by its ID.
    /// </summary>
    /// <param name="id">The ID of the role.</param>
    /// <returns>The result containing the role.</returns>
    public async Task<IActionResult> GetRoleById(long id)
    {
        var response = await _roleService.GetRoleByIdAsync(id);
        return Ok(response);
    }

    [HttpGet()]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<RoleResponseDTO>>))]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
    /// <summary>
    /// Gets all user roles.
    /// </summary>
    /// <returns>A list of all user roles.</returns>
    public async Task<IActionResult> GetAllRoles()
    {
        var response = await _roleService.GetAllRolesAsync();
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
    /// <summary>
    /// Deletes a user role by its ID.
    /// </summary>
    /// <param name="id">The ID of the role to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    public async Task<IActionResult> DeleteRole(long id)
    {
        AuditLog auditLog = new AuditLog
        {
            Action = UserAction.DeleteUserRole,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenService.GetUserEmail(Request),
            PerformedAgainst = $"Role ID: {id}",
            MacAddress = _tokenService.GetMacAddress(Request)
        };

        var response = await _roleService.DeleteRoleAsync(id, auditLog);
        return Ok(response);
    }
}
