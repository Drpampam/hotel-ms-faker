using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers;

[Route("api/v1/activation")]
[ApiController]
public class ActivationController : ControllerBase
{
    private readonly IActivationService _activationService;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _accessor;

    public ActivationController(IActivationService activationService, ITokenService tokenService, IHttpContextAccessor accessor)
    {
        _activationService = activationService;
        _tokenService = tokenService;
        _accessor = accessor;
    }

    [Authorize(Policy = "DeveloperPolicy")]
    [HttpPost("generate")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ActivationCodeResponseDTO>))]
    public async Task<IActionResult> GenerateActivationCode([FromBody] GenerateActivationCodeRequestDTO request)
    {
        var auditLog = new AuditLog
        {
            Action = UserAction.GenerateActivationCode,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            PerformerEmail = _tokenService.GetUserEmail(Request),
            PerformedAgainst = request.Email,
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
            MacAddress = _tokenService.GetMacAddress(Request)
        };
        var result = await _activationService.GenerateActivationCodeAsync(request, auditLog);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("activate")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ActivateTenantResponseDTO>))]
    public async Task<IActionResult> ActivateTenant([FromBody] ActivateTenantRequestDTO request)
    {
        var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        var result = await _activationService.ActivateTenantAsync(request, ip);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpGet("status/{tenantId}")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<SubscriptionStatusResponseDTO>))]
    public async Task<IActionResult> GetSubscriptionStatus(long tenantId)
    {
        var result = await _activationService.GetSubscriptionStatusAsync(tenantId);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [Authorize(Policy = "AdminPolicy")]
    [HttpPost("renew/{tenantId}")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    public async Task<IActionResult> RenewSubscription(long tenantId, [FromBody] RenewSubscriptionRequestDTO request)
    {
        var callerEmail = _tokenService.GetUserEmail(Request);
        var auditLog = new AuditLog
        {
            Action = UserAction.RenewSubscription,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenService.GetUserFullName(Request),
            PerformerEmail = callerEmail,
            PerformedAgainst = tenantId.ToString(),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
            MacAddress = _tokenService.GetMacAddress(Request)
        };
        var result = await _activationService.RenewSubscriptionAsync(tenantId, request.Code, callerEmail, auditLog);
        return result.Status ? Ok(result) : BadRequest(result);
    }
}
