using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public PropertiesController(IPropertyService propertyService,
            ITokenService tokenHelper,
            IHttpContextAccessor accessor)
        {
            _propertyService = propertyService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        // add permission authorization
        [HttpPost()]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> AddProperty(AddPropertyRequestDTO request)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.AddProperty,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = request.Name,
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };

            var response = await _propertyService.AddProperty(request, auditLog);
            return Ok(response);
        }

        // add permission authorization
        [HttpPut()]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> UpdateProperty(UpdatePropertyRequestDTO request)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.AddProperty,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = request.Name,
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };

            var response = await _propertyService.UpdateProperty(request, auditLog);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetProperty(long id)
        {
            var response = await _propertyService.GetById(id);
            return Ok(response);
        }

        [HttpGet("")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetTenantProperties(GetPropertiesInputDTO input)
        {
            var response = await _propertyService.GetTenantPropertyList(input);
            return Ok(response);
        }
    }
}
