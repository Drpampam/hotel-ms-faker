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

    [Authorize(Policy = "DeveloperPolicy")]
    [HttpPost("provision")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ProvisionTenantResponseDTO>))]
    public async Task<IActionResult> ProvisionTenant([FromBody] ProvisionTenantRequestDTO request)
    {
        var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        var result = await _activationService.ProvisionTenantAsync(request, ip);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("self-register")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<SelfRegisterResponseDTO>))]
    public async Task<IActionResult> SelfRegister([FromBody] SelfRegisterRequestDTO request)
    {
        var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        var result = await _activationService.SelfRegisterAsync(request, ip);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost("activate-my-account")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ActivateMyAccountResponseDTO>))]
    public async Task<IActionResult> ActivateMyAccount([FromBody] ActivateMyAccountRequestDTO request)
    {
        var callerEmail = _tokenService.GetUserEmail(Request);
        if (string.IsNullOrWhiteSpace(callerEmail))
            return Unauthorized();

        var result = await _activationService.ActivateMyAccountAsync(callerEmail, request);
        if (!result.Status) return BadRequest(result);

        // Issue fresh token + refresh token in headers (same pattern as Login)
        Response.Headers.TryAdd("Token", result.Data!.Token);
        Response.Headers.TryAdd("RefreshToken", result.Data.RefreshToken);
        Response.Headers.TryAdd("X-Tenant-Id", result.Data.TenantId.ToString());
        Response.Headers.TryAdd("Access-Control-Expose-Headers", "Token,RefreshToken,X-Tenant-Id");
        return Ok(result);
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

    [Authorize(Policy = "DeveloperPolicy")]
    [HttpPost("admin-renew/{tenantId}")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
    public async Task<IActionResult> AdminRenewSubscription(long tenantId, [FromBody] AdminRenewRequestDTO request)
    {
        var callerEmail = _tokenService.GetUserEmail(Request);
        var result = await _activationService.AdminRenewSubscriptionAsync(tenantId, request.PlanType, callerEmail);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [Authorize(Policy = "DeveloperPolicy")]
    [HttpGet("tenants")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<List<TenantSummaryDTO>>))]
    public async Task<IActionResult> GetAllTenants()
    {
        var result = await _activationService.GetAllTenantsAsync();
        return Ok(result);
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
