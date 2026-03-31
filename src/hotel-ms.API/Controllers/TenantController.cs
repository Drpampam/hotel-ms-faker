using hotelier_core_app.API.Helpers;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/tenants")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Developer")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantOnboardingService _tenantOnboardingService;
        private readonly ITokenService _tokenHelper;

        public TenantController(ITenantOnboardingService tenantOnboardingService, ITokenService tokenHelper)
        {
            _tenantOnboardingService = tenantOnboardingService;
            _tokenHelper = tokenHelper;
        }

        [HttpPost("{tenantId}/provision")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<TenantProvisionResponseDTO>))]
        public async Task<IActionResult> ProvisionTenant(long tenantId)
        {
            var performedBy = _tokenHelper.GetUserFullName(Request);
            var result = await _tenantOnboardingService.ProvisionTenantAsync(tenantId, performedBy);
            return Ok(result);
        }
    }
}
