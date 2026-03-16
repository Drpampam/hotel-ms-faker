using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.Configs;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;
        private readonly IOptions<JwtConfig> _jwtConfig;

        public UserController(
           IUserService userService,
           ITokenService tokenHelper,
           IHttpContextAccessor accessor,
           IOptions<JwtConfig> jwtConfig)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
            _jwtConfig = jwtConfig;
        }

        [HttpPut("activate-user")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> ActivateUser(ActivateUserRequestDTO model)
        {

            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.ActivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = model.Email,
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _userService.ActivateUser(model, auditLog);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("create-user")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> CreateUser(CreateUserRequestDTO model)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.CreateUser,
                DatePerformed = DateTime.UtcNow,
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformedAgainst = model.Email
            };
            BaseResponse response = await _userService.CreateUser(model, auditLog);
            return Ok(response);
        }

        [HttpPut("deactivate-user")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> DeactivateUser(DeactivateUserRequestDTO model)
        {

            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.DeactivateUser,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = model.Email,
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            BaseResponse response = await _userService.DeactivateUser(model, auditLog);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<LoginResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> Login(UserLoginRequestDTO model)
        {
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.UserLogin,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = model.Email,
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP"
            };
            var (response, refreshToken) = await _userService.Login(model, auditLog);
            if (response.Status)
            {
                Response.Headers.TryAdd("Token", _tokenHelper.GenerateJSONWebToken(
                    response.Data?.FullName ?? string.Empty,
                    response.Data?.Email ?? string.Empty,
                    response.Data?.Roles ?? Enumerable.Empty<string>().ToList()));
                Response.Headers.TryAdd("TokenExpiry", _jwtConfig.Value.TokenExpiryPeriod);
                Response.Headers.TryAdd("Access-Control-Expose-Headers", "Token,TokenExpiry,RefreshToken");
                Response.Headers.TryAdd("RefreshToken", refreshToken);
            }
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<RefreshTokenResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> RefreshToken(string currentRefreshToken)
        {
            string userEmail = _tokenHelper.GetUserEmail(Request);

            RefreshTokenRequestDTO model = new RefreshTokenRequestDTO { Email = userEmail, RefreshToken = currentRefreshToken };
            AuditLog auditLog = new AuditLog
            {
                Action = UserAction.RefreshToken,
                DatePerformed = DateTime.UtcNow,
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP"
            };
            var (response, refreshToken) = await _userService.RefreshToken(model, auditLog);
            if (response.Status)
            {
                Response.Headers.TryAdd("Token", _tokenHelper.GenerateJSONWebToken(
                    response.Data?.FullName ?? string.Empty,
                    response.Data?.Email ?? string.Empty,
                    response.Data?.Roles ?? Enumerable.Empty<string>().ToList()));
                Response.Headers.TryAdd("TokenExpiry", _jwtConfig.Value.TokenExpiryPeriod);
                Response.Headers.TryAdd("Access-Control-Expose-Headers", "Token,TokenExpiry,RefreshToken");
                Response.Headers.TryAdd("RefreshToken", refreshToken);
            }
            return Ok(response);
        }
        
        
        [HttpPut("reassign-role")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> ReassignRole(EditUserRolesRequestDTO model)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.ReassignRole,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = model.Email,
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };

            var response = await _userService.ReassignRole(model, auditLog);
            return Ok(response);
        }

        [HttpGet("get-user-by-email")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ApplicationUserDTO>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var response = await _userService.GetUserByEmail(email);
            return Ok(response);
        }

        [HttpPost("get-users")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<ApplicationUserDTO>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetUsers(PageParamsDTO model)
        {
            var response = await _userService.GetUsers(model);
            return Ok(response);
        }

        [HttpGet("get-assigned-modules")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<List<ModuleGroupDTO>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(BaseResponse))]
        public async Task<IActionResult> GetAssignedModules(string email)
        {
            var response = await _userService.GetAssignedModules(email);
            return Ok(response);
        }
    }
}
