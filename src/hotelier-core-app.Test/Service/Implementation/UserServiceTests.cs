using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Enums;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Implementation;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Service.Implementation
{
    public class UserServiceTests
    {
        private readonly IUserStore<ApplicationUser> _userStore = Substitute.For<IUserStore<ApplicationUser>>();
        private readonly IRoleStore<ApplicationRole> _roleStore = Substitute.For<IRoleStore<ApplicationRole>>();
        private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);
        private readonly RoleManager<ApplicationRole> _roleManager = Substitute.For<RoleManager<ApplicationRole>>(
            Substitute.For<IRoleStore<ApplicationRole>>(),
            null, null, null, null);
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserServiceTests()
        {
            var contextAccessor = Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var options = Substitute.For<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>();
            var schemes = Substitute.For<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            var confirmation = Substitute.For<IUserConfirmation<ApplicationUser>>();

            _signInManager = new SignInManager<ApplicationUser>(
                _userManager,
                contextAccessor,
                claimsFactory,
                options,
                logger,
                schemes,
                confirmation
            );
        }
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IDBCommandRepository<Tenant> _tenantCommandRepo = Substitute.For<IDBCommandRepository<Tenant>>();
        private readonly IEmailService _emailService = Substitute.For<IEmailService>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly IConfiguration _config = Substitute.For<IConfiguration>();
        private readonly IModuleService _moduleService = Substitute.For<IModuleService>();
        private readonly AuditLog _auditLog = new() { PerformedBy = "tester" };

        private UserService CreateService() => new(
            _userManager,
            _roleManager,
            _signInManager,
            _auditLogCommandRepo,
            _tenantCommandRepo,
            _emailService,
            _mapper,
            _config,
            _moduleService
        );

        [Fact]
        public async Task ActivateUser_ShouldReturnFailure_WhenUserNotFound()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();
            var dto = new ActivateUserRequestDTO { Email = "notfound@test.com", Role = UserRole.Admin };
            var result = await service.ActivateUser(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task ActivateUser_ShouldReturnSuccess_WhenUserFoundAndRoleExists()
        {
            var user = new ApplicationUser { Email = "found@test.com", Status = "Inactive" };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);
            _userManager.AddToRoleAsync(user, Arg.Any<string>()).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new ActivateUserRequestDTO { Email = "found@test.com", Role = UserRole.Admin };
            var result = await service.ActivateUser(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task ActivateUser_ShouldReturnSuccess_WhenUserFoundAndRoleDoesNotExist()
        {
            var user = new ApplicationUser { Email = "found@test.com", Status = "Inactive" };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(false);
            _roleManager.CreateAsync(Arg.Any<ApplicationRole>()).Returns(IdentityResult.Success);
            _mapper.Map<ApplicationRole>(Arg.Any<ActivateUserRequestDTO>()).Returns(new ApplicationRole());
            _userManager.AddToRoleAsync(user, Arg.Any<string>()).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new ActivateUserRequestDTO { Email = "found@test.com", Role = UserRole.Admin };
            var result = await service.ActivateUser(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task ActivateUser_ShouldReturnFailure_WhenRoleAssignmentFails()
        {
            var user = new ApplicationUser { Email = "found@test.com", Status = "Inactive" };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);
            _userManager.AddToRoleAsync(user, Arg.Any<string>()).Returns(IdentityResult.Failed());
            var service = CreateService();
            var dto = new ActivateUserRequestDTO { Email = "found@test.com", Role = UserRole.Admin };
            var result = await service.ActivateUser(dto, _auditLog);
            Assert.False(result.Status);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnFailure_WhenUserExists()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            var service = CreateService();
            var dto = new CreateUserRequestDTO { Email = "exists@test.com", Role = UserRole.Admin, Password = "pass" };
            var result = await service.CreateUser(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserExist, result.Message);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnFailure_WhenIdentityResultFails()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            _mapper.Map<ApplicationUser>(Arg.Any<CreateUserRequestDTO>()).Returns(new ApplicationUser());
            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed());
            var service = CreateService();
            var dto = new CreateUserRequestDTO { Email = "new@test.com", Role = UserRole.Admin, Password = "pass" };
            var result = await service.CreateUser(dto, _auditLog);
            Assert.False(result.Status);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnSuccess_WhenUserCreatedAndRoleExists()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            _mapper.Map<ApplicationUser>(Arg.Any<CreateUserRequestDTO>()).Returns(new ApplicationUser());
            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);
            _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new CreateUserRequestDTO { Email = "new@test.com", Role = UserRole.Admin, Password = "pass" };
            var result = await service.CreateUser(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnSuccess_WhenUserCreatedAndRoleDoesNotExist()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            _mapper.Map<ApplicationUser>(Arg.Any<CreateUserRequestDTO>()).Returns(new ApplicationUser());
            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(false);
            _roleManager.CreateAsync(Arg.Any<ApplicationRole>()).Returns(IdentityResult.Success);
            _mapper.Map<ApplicationRole>(Arg.Any<CreateUserRequestDTO>()).Returns(new ApplicationRole());
            _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new CreateUserRequestDTO { Email = "new@test.com", Role = UserRole.Admin, Password = "pass" };
            var result = await service.CreateUser(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnFailure_WhenRoleAssignmentFails()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            _mapper.Map<ApplicationUser>(Arg.Any<CreateUserRequestDTO>()).Returns(new ApplicationUser());
            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);
            _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed());
            _userManager.DeleteAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new CreateUserRequestDTO { Email = "new@test.com", Role = UserRole.Admin, Password = "pass" };
            var result = await service.CreateUser(dto, _auditLog);
            Assert.False(result.Status);
        }

        [Fact]
        public async Task DeactivateUser_ShouldReturnFailure_WhenUserNotFound()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();
            var dto = new DeactivateUserRequestDTO { Email = "notfound@test.com", Status = UserStatus.Suspended };
            var result = await service.DeactivateUser(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task DeactivateUser_ShouldReturnFailure_WhenUserAlreadyInactive()
        {
            var user = new ApplicationUser { Email = "inactive@test.com", Status = UserStatus.Active.ToString() };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            var service = CreateService();
            var dto = new DeactivateUserRequestDTO { Email = "inactive@test.com", Status = UserStatus.Active };
            var result = await service.DeactivateUser(dto, _auditLog);
            Assert.False(result.Status);
        }

        [Fact]
        public async Task DeactivateUser_ShouldReturnSuccess_WhenUserDeactivated()
        {
            var user = new ApplicationUser { Email = "active@test.com", Status = UserStatus.Active.ToString(), IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new DeactivateUserRequestDTO { Email = "active@test.com", Status = UserStatus.Suspended };
            var result = await service.DeactivateUser(dto, _auditLog);
            Assert.True(result.Status);
            Assert.Equal(ResponseMessages.UserDeactivated, result.Message);
        }

        [Fact]
        public async Task DeactivateUser_ShouldSetIsDeleted_WhenStatusIsSackedOrResigned()
        {
            var user = new ApplicationUser { Email = "sacked@test.com", Status = UserStatus.Active.ToString(), IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new DeactivateUserRequestDTO { Email = "sacked@test.com", Status = UserStatus.Sacked };
            await service.DeactivateUser(dto, _auditLog);
            Assert.True(user.IsDeleted);
        }

        [Fact]
        public async Task DeactivateUser_ShouldReturnFailure_WhenUpdateFails()
        {
            var user = new ApplicationUser { Email = "fail@test.com", Status = UserStatus.Active.ToString(), IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Failed());
            var service = CreateService();
            var dto = new DeactivateUserRequestDTO { Email = "fail@test.com", Status = UserStatus.Suspended };
            var result = await service.DeactivateUser(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.OperationFailed, result.Message);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenUserNotFound()
        {
            _userManager.Users.Returns(new List<ApplicationUser>().AsQueryable());
            var service = CreateService();
            var dto = new UserLoginRequestDTO { Email = "notfound@test.com", Password = "pass", RememberMe = false };
            var result = await service.Login(dto, _auditLog);
            Assert.False(result.Item1.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Item1.Message);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenUserInactive()
        {
            var user = new ApplicationUser { Email = "inactive@test.com", IsActive = false, EmailConfirmed = true };
            _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());
            var service = CreateService();
            var dto = new UserLoginRequestDTO { Email = "inactive@test.com", Password = "pass", RememberMe = false };
            var result = await service.Login(dto, _auditLog);
            Assert.False(result.Item1.Status);
            Assert.Equal(ResponseMessages.UserInactive, result.Item1.Message);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenEmailNotConfirmed()
        {
            var user = new ApplicationUser { Email = "unconfirmed@test.com", IsActive = true, EmailConfirmed = false };
            _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());
            var service = CreateService();
            var dto = new UserLoginRequestDTO { Email = "unconfirmed@test.com", Password = "pass", RememberMe = false };
            var result = await service.Login(dto, _auditLog);
            Assert.False(result.Item1.Status);
            Assert.Equal(ResponseMessages.UserEmailNotConfirmed, result.Item1.Message);
        }

        [Fact]
        public async Task GetAssignedModules_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();

            var result = await service.GetAssignedModules("missing@test.com");

            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task GetAssignedModules_ShouldReturnSuccess_WhenUserExists()
        {
            var user = new ApplicationUser { Email = "user@test.com" };
            var modules = new List<ModuleGroupDTO>();
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.GetRolesAsync(user).Returns(["Admin"]);
            _moduleService.GetAssignedModules(Arg.Any<List<string>>())
                .Returns(BaseResponse<List<ModuleGroupDTO>>.Success(modules));

            var service = CreateService();
            var result = await service.GetAssignedModules("user@test.com");

            Assert.True(result.Status);
            Assert.Same(modules, result.Data);
            Assert.Equal(ResponseMessages.ModulesRetrieved, result.Message);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();

            var result = await service.GetUserByEmail("missing@test.com");

            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnSuccess_WhenUserExists()
        {
            var user = new ApplicationUser { Email = "user@test.com", FullName = "Test User" };
            var dto = new ApplicationUserDTO { Email = "user@test.com", FullName = "Test User" };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _mapper.Map<ApplicationUserDTO>(user).Returns(dto);
            var service = CreateService();

            var result = await service.GetUserByEmail("user@test.com");

            Assert.True(result.Status);
            Assert.Equal("user@test.com", result.Data.Email);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnFailure_WhenUserNotFound()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = "notfound@test.com", Roles = ["Admin"] };
            var result = await service.ReassignRole(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnFailure_WhenUserInactive()
        {
            var user = new ApplicationUser { Email = "inactive@test.com", IsActive = false };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = "inactive@test.com", Roles = ["Admin"] };
            var result = await service.ReassignRole(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserInactive, result.Message);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnFailure_WhenRoleDoesNotExist()
        {
            var user = new ApplicationUser { Email = "active@test.com", IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(false);
            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = "active@test.com", Roles = ["NonExistentRole"] };
            var result = await service.ReassignRole(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.RoleNotExist, result.Message);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnFailure_WhenEmailIsEmpty()
        {
            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = string.Empty, Roles = ["Admin"] };

            var result = await service.ReassignRole(dto, _auditLog);

            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnFailure_WhenRemoveRolesFails()
        {
            var user = new ApplicationUser { Email = "active@test.com", IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _roleManager.RoleExistsAsync("Admin").Returns(true);
            _userManager.GetRolesAsync(user).Returns(["OldRole"]);
            _userManager.RemoveFromRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Failed());

            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = "active@test.com", Roles = ["Admin"] };

            var result = await service.ReassignRole(dto, _auditLog);

            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.RoleReassignmentError, result.Message);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnFailure_WhenAddRolesFails()
        {
            var user = new ApplicationUser { Email = "active@test.com", IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _roleManager.RoleExistsAsync("Admin").Returns(true);
            _userManager.GetRolesAsync(user).Returns(["OldRole"]);
            _userManager.RemoveFromRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Success);
            _userManager.AddToRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Failed());

            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = "active@test.com", Roles = ["Admin"] };

            var result = await service.ReassignRole(dto, _auditLog);

            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.RoleReassignmentError, result.Message);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnSuccess_WhenRoleReassignmentSucceeds()
        {
            var user = new ApplicationUser { Email = "active@test.com", IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _roleManager.RoleExistsAsync("Admin").Returns(true);
            _userManager.GetRolesAsync(user).Returns(["OldRole"]);
            _userManager.RemoveFromRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Success);
            _userManager.AddToRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Success);

            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = "active@test.com", Roles = ["Admin"] };

            var result = await service.ReassignRole(dto, _auditLog);

            Assert.True(result.Status);
            Assert.Equal(ResponseMessages.RoleUpdated, result.Message);
            await _auditLogCommandRepo.Received(1).AddAsync(_auditLog);
            await _auditLogCommandRepo.Received(1).SaveAsync();
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnFailure_WhenUserNotFound()
        {
            _userManager.Users.Returns(new List<ApplicationUser>().AsQueryable());
            var service = CreateService();
            var dto = new RefreshTokenRequestDTO { Email = "notfound@test.com", RefreshToken = "token" };
            var result = await service.RefreshToken(dto, _auditLog);
            Assert.False(result.Item1.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Item1.Message);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnFailure_WhenRefreshTokenDoesNotMatch()
        {
            var user = new ApplicationUser { Email = "user@test.com", RefreshToken = "oldtoken" };
            _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());
            var service = CreateService();
            var dto = new RefreshTokenRequestDTO { Email = "user@test.com", RefreshToken = "wrongtoken" };
            var result = await service.RefreshToken(dto, _auditLog);
            Assert.False(result.Item1.Status);
            Assert.Equal(ResponseMessages.CantVerifyRefreshToken, result.Item1.Message);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnSuccess_WhenRefreshTokenMatches()
        {
            var user = new ApplicationUser { Email = "user@test.com", RefreshToken = "token", FullName = "Test User" };
            _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());
            _userManager.GetRolesAsync(user).Returns(["Admin"]); // simplified collection initialization
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new RefreshTokenRequestDTO { Email = "user@test.com", RefreshToken = "token" };
            var result = await service.RefreshToken(dto, _auditLog);
            Assert.True(result.Item1.Status);
            Assert.Equal(ResponseMessages.LoginSuccessful, result.Item1.Message);
            Assert.Equal("user@test.com", result.Item1.Data.Email);
            Assert.Equal("Test User", result.Item1.Data.FullName);
            Assert.Contains("Admin", result.Item1.Data.Roles);
            Assert.False(string.IsNullOrEmpty(result.Item2));
        }

        [Fact]
        public async Task UpdateUserName_ShouldReturnFailure_WhenUserNotFound()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();
            var dto = new EditUserNameRequestDTO { Email = "notfound@test.com", Name = "New Name" };
            var result = await service.UpdateUserName(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task UpdateUserName_ShouldReturnFailure_WhenUserInactive()
        {
            var user = new ApplicationUser { Email = "inactive@test.com", IsActive = false };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            var service = CreateService();
            var dto = new EditUserNameRequestDTO { Email = "inactive@test.com", Name = "New Name" };
            var result = await service.UpdateUserName(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserInactive, result.Message);
        }

        [Fact]
        public async Task UpdateUserName_ShouldReturnSuccess_WhenUserActive()
        {
            var user = new ApplicationUser { Email = "active@test.com", IsActive = true };
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
            var service = CreateService();
            var dto = new EditUserNameRequestDTO { Email = "active@test.com", Name = "New Name" };
            var result = await service.UpdateUserName(dto, _auditLog);
            Assert.True(result.Status);
            Assert.Equal(ResponseMessages.UpdateSuccessful, result.Message);
        }

        [Fact]
        public async Task GetAssignedModules_ShouldReturnFailure_WhenUserNotFound()
        {
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();

            var result = await service.GetAssignedModules("missing@test.com");

            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task ReassignRole_ShouldReturnSuccess_WhenUserIsActiveAndRolesAreValid()
        {
            var user = new ApplicationUser { Email = "active@test.com", IsActive = true };
            _userManager.FindByEmailAsync("active@test.com").Returns(user);
            _roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);
            _userManager.GetRolesAsync(user).Returns(new List<string> { "OldRole" });
            _userManager.RemoveFromRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Success);
            _userManager.AddToRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Success);

            var service = CreateService();
            var dto = new EditUserRolesRequestDTO { Email = "active@test.com", Roles = ["Admin", "Manager"] };
            var result = await service.ReassignRole(dto, _auditLog);

            Assert.True(result.Status);
            Assert.Equal(ResponseMessages.RoleUpdated, result.Message);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenPasswordSignInFails()
        {
            var user = new ApplicationUser { Email = "user@test.com", IsActive = true, EmailConfirmed = true };
            _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());
            _userManager.CheckPasswordAsync(user, Arg.Any<string>()).Returns(false);

            var service = CreateService();
            var dto = new UserLoginRequestDTO { Email = "user@test.com", Password = "bad-pass", RememberMe = false };
            var result = await service.Login(dto, _auditLog);

            Assert.False(result.Item1.Status);
            Assert.Equal(ResponseMessages.InvalidCredential, result.Item1.Message);
            Assert.Equal(string.Empty, result.Item2);
        }
    }
}
