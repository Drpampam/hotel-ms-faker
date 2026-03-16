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
/// Controller for managing subscription plans and tenant assignments.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionController"/> class.
    /// </summary>
    /// <param name="subscriptionService">Service for subscription operations.</param>
    /// <param name="tokenService">Service for token operations.</param>
    /// <param name="accessor">HTTP context accessor.</param>
    public SubscriptionController(ISubscriptionService subscriptionService, ITokenService tokenService, IHttpContextAccessor accessor)
    {
        this._subscriptionService = subscriptionService;
        this._tokenService = tokenService;
        this._accessor = accessor;
    }

    [Authorize(Policy = "DeveloperPolicy")]
    [HttpPost("create-plan")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(BaseResponse))]
    /// <summary>
    /// Creates a new subscription plan.
    /// </summary>
    /// <param name="request">The subscription plan creation request.</param>
    /// <returns>The result of the creation operation.</returns>
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanDTO request)
    {
        AuditLog auditLog = new AuditLog
        {
            Action = UserAction.CreateSubscriptionPlan,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenService.GetUserEmail(Request),
            PerformedAgainst = request.Name,
            MacAddress = _tokenService.GetMacAddress(Request)
        };
        var response = await _subscriptionService.CreateSubscriptionPlanAsync(request, auditLog);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<SubscriptionPlanResponseDTO>))]
    [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(BaseResponse))]
    /// <summary>
    /// Gets a subscription plan by its ID.
    /// </summary>
    /// <param name="id">The ID of the subscription plan.</param>
    /// <returns>The result containing the subscription plan.</returns>
    public async Task<IActionResult> GetSubscriptionPlanById(long id)
    {
        var response = await _subscriptionService.GetSubscriptionPlanByIdAsync(id);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(List<SubscriptionPlanResponseDTO>))]
    /// <summary>
    /// Gets all subscription plans.
    /// </summary>
    /// <returns>A list of all subscription plans.</returns>
    public async Task<IActionResult> GetAllSubscriptionPlans()
    {
        var response = await _subscriptionService.GetAllSubscriptionPlansAsync();
        return Ok(response);
    }

    [Authorize(Policy = "DeveloperPolicy")]
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(BaseResponse))]
    /// <summary>
    /// Deletes a subscription plan by its ID.
    /// </summary>
    /// <param name="id">The ID of the subscription plan to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    public async Task<IActionResult> DeleteSubscriptionPlan(long id)
    {
        AuditLog auditLog = new AuditLog
        {
            Action = UserAction.DeleteSubscriptionPlan,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenService.GetUserEmail(Request),
            PerformedAgainst = id.ToString(),
            MacAddress = _tokenService.GetMacAddress(Request)
        };
        var response = await _subscriptionService.DeleteSubscriptionPlanAsync(id, auditLog);
        return response.Status ? Ok(response) : BadRequest(response);
    }

    [HttpPost("subscribe")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(BaseResponse))]
    /// <summary>
    /// Assigns a subscription plan to a tenant.
    /// </summary>
    /// <param name="request">The assignment request.</param>
    /// <returns>The result of the assignment operation.</returns>
    public async Task<IActionResult> AssignSubscriptionPlanToTenant([FromBody] AssignSubscriptionPlanDTO request)
    {
        AuditLog auditLog = new AuditLog
        {
            Action = UserAction.ActivateSubscriptionPlan,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenService.GetUserEmail(Request),
            PerformedAgainst = request.TenantId.ToString(),
            MacAddress = _tokenService.GetMacAddress(Request)
        };
        var response = await _subscriptionService.AssignSubscriptionPlanToTenantAsync(request, auditLog);
        return response.Status ? Ok(response) : BadRequest(response);
    }
}