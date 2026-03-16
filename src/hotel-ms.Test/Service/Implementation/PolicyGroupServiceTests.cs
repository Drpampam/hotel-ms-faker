using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Service.Implementation
{
    public class PolicyGroupServiceTests
    {
        private readonly IDBCommandRepository<PolicyGroup> _policyGroupCommandRepo = Substitute.For<IDBCommandRepository<PolicyGroup>>();
        private readonly IDBQueryRepository<PolicyGroup> _policyGroupQueryRepo = Substitute.For<IDBQueryRepository<PolicyGroup>>();
        private readonly IUserStore<ApplicationUser> _userStore = Substitute.For<IUserStore<ApplicationUser>>();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IDBCommandRepository<ApplicationUserPolicyGroup> _userPolicyCommandRepo = Substitute.For<IDBCommandRepository<ApplicationUserPolicyGroup>>();
        private readonly IDBQueryRepository<ApplicationUserPolicyGroup> _userPolicyQueryRepo = Substitute.For<IDBQueryRepository<ApplicationUserPolicyGroup>>();
        private readonly IDBQueryRepository<ModuleGroup> _moduleGroupQueryRepo = Substitute.For<IDBQueryRepository<ModuleGroup>>();
        private readonly IDBQueryRepository<Permission> _permissionQueryRepo = Substitute.For<IDBQueryRepository<Permission>>();
        private readonly IDBQueryRepository<PolicyModulePermission> _pmpQueryRepo = Substitute.For<IDBQueryRepository<PolicyModulePermission>>();
        private readonly IDBCommandRepository<PolicyModulePermission> _pmpCommandRepo = Substitute.For<IDBCommandRepository<PolicyModulePermission>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly AuditLog _auditLog = new() { PerformedBy = "tester", PerformerEmail = "test@email.com" };

        public PolicyGroupServiceTests()
        {
            _userManager = Substitute.For<UserManager<ApplicationUser>>(
                _userStore, null, null, null, null, null, null, null, null);
        }

        private PolicyGroupService CreateService() => new(
            _policyGroupCommandRepo,
            _policyGroupQueryRepo,
            _userManager,
            _auditLogCommandRepo,
            _userPolicyCommandRepo,
            _userPolicyQueryRepo,
            _moduleGroupQueryRepo,
            _permissionQueryRepo,
            _pmpQueryRepo,
            _pmpCommandRepo,
            _mapper);
            
        [Fact]
        public async Task GetAllPermission_ShouldReturnMappedPermissions()
        {
            var permissions = new List<Permission> { new Permission(), new Permission() };
            _permissionQueryRepo.GetAllAsync().Returns(permissions);
            _mapper.Map<List<PermissionDTO>>(permissions).Returns([new PermissionDTO(), new PermissionDTO()]);
            var service = CreateService();
            var result = await service.GetAllPermission();
            Assert.True(result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllPermission_ShouldReturnEmptyList_WhenNoPermissionsExist()
        {
            _permissionQueryRepo.GetAllAsync().Returns([]);
            _mapper.Map<List<PermissionDTO>>(Arg.Any<List<Permission>>()).Returns([]);
            var service = CreateService();
            var result = await service.GetAllPermission();
            Assert.True(result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task AddPolicyGroup_ShouldReturnFailure_WhenPolicyGroupExists()
        {
            _policyGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<PolicyGroup, bool>>>()).Returns(new PolicyGroup());
            var service = CreateService();
            var dto = new AddPolicyGroupDTO { Name = "Test", TenantId = 1 };
            var result = await service.AddPolicyGroup(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.PolicyGroupExists, result.Message);
        }

        [Fact]
        public async Task AddPolicyGroup_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            _policyGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<PolicyGroup, bool>>>()).Returns((PolicyGroup)null);
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser)null);
            var service = CreateService();
            var dto = new AddPolicyGroupDTO { Name = "Test", TenantId = 1 };
            var result = await service.AddPolicyGroup(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.UserDoesNotExist, result.Message);
        }

        [Fact]
        public async Task AddPolicyGroup_ShouldReturnSuccess_WhenValid()
        {
            _policyGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<PolicyGroup, bool>>>()).Returns((PolicyGroup)null);
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _policyGroupCommandRepo.Add(Arg.Any<PolicyGroup>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _policyGroupCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.SaveAsync().Returns(1);
            var service = CreateService();
            var dto = new AddPolicyGroupDTO { Name = "Test", TenantId = 1 };
            var result = await service.AddPolicyGroup(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task AddPolicyGroup_ShouldReturnFailure_WhenDtoIsNull()
        {
            var service = CreateService();
            var result = await service.AddPolicyGroup(null, _auditLog);
            Assert.False(result.Status);
            Assert.Contains("User does not exist", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddPolicyGroup_ShouldReturnFailure_WhenAuditLogIsNull()
        {
            var service = CreateService();
            var dto = new AddPolicyGroupDTO { Name = "Test", TenantId = 1 };
            var result = await service.AddPolicyGroup(dto, null);
            Assert.False(result.Status);
            Assert.Contains("User does not exist", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddPolicyGroup_ShouldReturnFailure_WhenSaveFails()
        {
            _policyGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<PolicyGroup, bool>>>()).Returns((PolicyGroup)null);
            _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _policyGroupCommandRepo.Add(Arg.Any<PolicyGroup>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _policyGroupCommandRepo.SaveAsync().Returns(0);
            _auditLogCommandRepo.SaveAsync().Returns(1);
            var service = CreateService();
            var dto = new AddPolicyGroupDTO { Name = "Test", TenantId = 1 };
            var result = await service.AddPolicyGroup(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Contains("failed", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAllPermission_ShouldHandleExceptionAndReturnFailure()
        {
            _permissionQueryRepo.GetAllAsync().Returns<Task<IEnumerable<Permission>>>(x => throw new Exception("DB error"));
            var service = CreateService();
            var result = await service.GetAllPermission();
            Assert.False(result.Status);
            Assert.Contains("error", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
