using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Implementation;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class RoleServiceTests
    {
        private readonly IDBCommandRepository<ApplicationRole> _roleCommandRepo = Substitute.For<IDBCommandRepository<ApplicationRole>>();
        private readonly IDBQueryRepository<ApplicationRole> _roleQueryRepo = Substitute.For<IDBQueryRepository<ApplicationRole>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IDBCommandRepository<RolePolicyGroup> _rolePolicyGroupCommandRepo = Substitute.For<IDBCommandRepository<RolePolicyGroup>>();
        private readonly IDBQueryRepository<RolePolicyGroup> _rolePolicyGroupQueryRepo = Substitute.For<IDBQueryRepository<RolePolicyGroup>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly AuditLog _auditLog = new() { PerformedBy = "tester" };

        private RoleService CreateService() => new(
            _roleCommandRepo,
            _roleQueryRepo,
            _auditLogCommandRepo,
            _rolePolicyGroupCommandRepo,
            _rolePolicyGroupQueryRepo,
            _mapper);

        [Fact]
        public async Task CreateRoleAsync_ShouldReturnFailure_WhenRoleExists()
        {
            _roleQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<ApplicationRole, bool>>>()).Returns(new ApplicationRole());
            var service = CreateService();
            var dto = new CreateRoleRequestDTO { RoleName = "Admin", TenantId = 1 };
            var result = await service.CreateRoleAsync(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.RoleExist, result.Message);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldReturnFailure_WhenTenantIdIsNull()
        {
            var service = CreateService();
            var dto = new CreateRoleRequestDTO { RoleName = "Admin", TenantId = null, PolicyGroupIds = new List<long> { 1 } };

            var result = await service.CreateRoleAsync(dto, _auditLog);

            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvalidData, result.StatusCode);
            Assert.Equal("TenantId is required.", result.Message);
            await _roleCommandRepo.DidNotReceive().AddAsync(Arg.Any<ApplicationRole>());
            await _rolePolicyGroupCommandRepo.DidNotReceive().AddAsync(Arg.Any<RolePolicyGroup>());
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldReturnSuccess_WhenRoleDoesNotExist()
        {
            _roleQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<ApplicationRole, bool>>>()).Returns((ApplicationRole)null);
            var roleEntity = new ApplicationRole();
            _mapper.Map<ApplicationRole>(Arg.Any<CreateRoleRequestDTO>()).Returns(roleEntity);
            _roleCommandRepo.AddAsync(roleEntity).Returns(Task.FromResult<object>(roleEntity));
            _roleCommandRepo.SaveAsync().Returns(1);
            _rolePolicyGroupCommandRepo.AddAsync(Arg.Any<RolePolicyGroup>()).Returns(callInfo => Task.FromResult<object>(callInfo.Arg<RolePolicyGroup>()));
            _rolePolicyGroupCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.AddAsync(Arg.Any<AuditLog>()).Returns(callInfo => Task.FromResult<object>(callInfo.Arg<AuditLog>()));
            _auditLogCommandRepo.SaveAsync().Returns(1);
            var service = CreateService();
            var dto = new CreateRoleRequestDTO { RoleName = "Admin", TenantId = 1, PolicyGroupIds = new List<long> { 1, 2 } };
            var result = await service.CreateRoleAsync(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldReturnFailure_WhenRoleNotFound()
        {
            _roleQueryRepo.FindAsync(Arg.Any<long>()).Returns((ApplicationRole)null);
            var service = CreateService();
            var dto = new UpdateRoleRequestDTO { Id = 1, RoleName = "Admin" };
            var result = await service.UpdateRoleAsync(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.RoleNotExist, result.Message);
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldReturnSuccess_WhenRoleFound()
        {
            var role = new ApplicationRole { Id = 1, Name = "Old" };
            _roleQueryRepo.FindAsync(Arg.Any<long>()).Returns(role);
            _roleCommandRepo.UpdateAsync(role).Returns(Task.CompletedTask);
            _auditLogCommandRepo.AddAsync(Arg.Any<AuditLog>()).Returns(callInfo => Task.FromResult<object>(callInfo.Arg<AuditLog>()));
            _auditLogCommandRepo.SaveAsync().Returns(1);
            var service = CreateService();
            var dto = new UpdateRoleRequestDTO { Id = 1, RoleName = "Admin" };
            var result = await service.UpdateRoleAsync(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task GetRoleByIdAsync_ShouldReturnFailure_WhenRoleNotFound()
        {
            _roleQueryRepo.FindAsync(Arg.Any<long>()).Returns((ApplicationRole)null);
            var service = CreateService();
            var result = await service.GetRoleByIdAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.RoleNotExist, result.Message);
        }

        [Fact]
        public async Task GetRoleByIdAsync_ShouldReturnSuccess_WhenRoleFound()
        {
            var role = new ApplicationRole { Id = 1, Name = "Admin" };
            _roleQueryRepo.FindAsync(Arg.Any<long>()).Returns(role);
            _mapper.Map<RoleResponseDTO>(role).Returns(new RoleResponseDTO());
            var service = CreateService();
            var result = await service.GetRoleByIdAsync(1);
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetAllRolesAsync_ShouldReturnMappedRoles()
        {
            var roles = new List<ApplicationRole> { new ApplicationRole { Id = 1 }, new ApplicationRole { Id = 2 } };
            _roleQueryRepo.GetAllAsync().Returns(roles);
            _mapper.Map<List<RoleResponseDTO>>(roles).Returns(new List<RoleResponseDTO> { new RoleResponseDTO(), new RoleResponseDTO() });
            var service = CreateService();
            var result = await service.GetAllRolesAsync();
            Assert.True(result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllRolesAsync_ShouldReturnEmptyList_WhenNoRolesExist()
        {
            _roleQueryRepo.GetAllAsync().Returns(new List<ApplicationRole>());
            _mapper.Map<List<RoleResponseDTO>>(Arg.Any<List<ApplicationRole>>()).Returns(new List<RoleResponseDTO>());
            var service = CreateService();
            var result = await service.GetAllRolesAsync();
            Assert.True(result.Status);
            Assert.Empty(result.Data);
        }
    }
}
