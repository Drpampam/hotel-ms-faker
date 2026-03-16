using hotelier_core_app.API.Controllers;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Service.Interface;
using hotelier_core_app.Model.Entities; // Add this for AuditLog
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using System.Threading.Tasks;
using hotelier_core_app.Model.Configs;

public class UserControllerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IHttpContextAccessor _accessor = Substitute.For<IHttpContextAccessor>();
    private readonly IOptions<JwtConfig> _jwtConfig = Substitute.For<IOptions<JwtConfig>>();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _jwtConfig.Value.Returns(new JwtConfig { TokenExpiryPeriod = "3600", TokenKey = "dummyKey", TokenIssuer = "dummyIssuer" });
        _controller = new UserController(_userService, _tokenService, _accessor, _jwtConfig);
    }

    [Fact]
    public async Task ActivateUser_ReturnsOk_WhenSuccess()
    {
        _userService.ActivateUser(Arg.Any<ActivateUserRequestDTO>(), Arg.Any<AuditLog>())
            .Returns(Task.FromResult(new BaseResponse { Status = true }));
        var result = await _controller.ActivateUser(new ActivateUserRequestDTO { Email = "test@example.com" });
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateUser_ReturnsOk_WhenSuccess()
    {
        _userService.CreateUser(Arg.Any<CreateUserRequestDTO>(), Arg.Any<AuditLog>())
            .Returns(Task.FromResult(new BaseResponse { Status = true }));
        var result = await _controller.CreateUser(new CreateUserRequestDTO { Email = "test@example.com" });
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeactivateUser_ReturnsOk_WhenSuccess()
    {
        _userService.DeactivateUser(Arg.Any<DeactivateUserRequestDTO>(), Arg.Any<AuditLog>())
            .Returns(Task.FromResult(new BaseResponse { Status = true }));
        var result = await _controller.DeactivateUser(new DeactivateUserRequestDTO { Email = "test@example.com" });
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenSuccess()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _userService.Login(Arg.Any<UserLoginRequestDTO>(), Arg.Any<AuditLog>())
            .Returns(Task.FromResult((new BaseResponse<LoginResponseDTO> { Status = true, Data = new LoginResponseDTO { FullName = "Test", Email = "test@example.com" } }, "refreshToken")));
        var result = await _controller.Login(new UserLoginRequestDTO { Email = "test@example.com", Password = "pass" });
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenSuccess()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _tokenService.GetUserEmail(Arg.Any<HttpRequest>()).Returns("test@example.com");
        _userService.RefreshToken(Arg.Any<RefreshTokenRequestDTO>(), Arg.Any<AuditLog>())
            .Returns(Task.FromResult((new BaseResponse<RefreshTokenResponseDTO> { Status = true, Data = new RefreshTokenResponseDTO() }, "refreshToken")));
        var result = await _controller.RefreshToken("currentRefreshToken");
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ActivateUser_ThrowsException_WhenModelIsNull()
    {
        await Assert.ThrowsAsync<System.NullReferenceException>(() => _controller.ActivateUser(null));
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenLoginFails()
    {
        _userService.Login(Arg.Any<UserLoginRequestDTO>(), Arg.Any<AuditLog>())
            .Returns((new BaseResponse<LoginResponseDTO> { Status = false }, null));
        var result = await _controller.Login(new UserLoginRequestDTO { Email = "wrong@example.com", Password = "bad" });
        Assert.IsType<OkObjectResult>(result);
    }
}
