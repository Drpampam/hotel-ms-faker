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
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class ModuleServiceTests
    {
        private readonly IDBCommandRepository<ModuleGroup> _moduleGroupCommandRepo = Substitute.For<IDBCommandRepository<ModuleGroup>>();
        private readonly IDBQueryRepository<ModuleGroup> _moduleGroupQueryRepo = Substitute.For<IDBQueryRepository<ModuleGroup>>();
        private readonly IDBCommandRepository<Module> _moduleCommandRepo = Substitute.For<IDBCommandRepository<Module>>();
        private readonly IDBQueryRepository<Module> _moduleQueryRepo = Substitute.For<IDBQueryRepository<Module>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly AuditLog _auditLog = new() { PerformedBy = "tester" };

        private ModuleService CreateService() => new(
            _moduleGroupCommandRepo,
            _moduleGroupQueryRepo,
            _moduleCommandRepo,
            _moduleQueryRepo,
            _auditLogCommandRepo,
            _mapper);

        [Fact]
        public async Task CreateModule_ShouldReturnSuccess_WhenModuleDoesNotExist()
        {
            _moduleQueryRepo.GetBy(Arg.Any<System.Linq.Expressions.Expression<Func<Module, bool>>>()).Returns(new List<Module>());
            _moduleCommandRepo.Add(Arg.Any<Module>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _moduleCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.SaveAsync().Returns(1);

            var service = CreateService();
            var dto = new CreateModuleDTO { Name = "Test", ModuleGroupId = 1, Description = "desc", Url = "url" };
            var result = await service.CreateModule(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task CreateModule_ShouldReturnFailure_WhenModuleExists()
        {
            _moduleQueryRepo.GetBy(Arg.Any<System.Linq.Expressions.Expression<Func<Module, bool>>>()).Returns(new List<Module> { new Module() });
            var service = CreateService();
            var dto = new CreateModuleDTO { Name = "Test", ModuleGroupId = 1, Description = "desc", Url = "url" };
            var result = await service.CreateModule(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleExist, result.Message);
        }

        [Fact]
        public async Task CreateModuleGroup_ShouldReturnSuccess_WhenGroupDoesNotExist()
        {
            _moduleGroupQueryRepo.GetBy(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, bool>>>()).Returns(new List<ModuleGroup>());
            _moduleGroupCommandRepo.Add(Arg.Any<ModuleGroup>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _moduleGroupCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.SaveAsync().Returns(1);

            var service = CreateService();
            var dto = new CreateModuleGroupDTO { Name = "Group", Description = "desc", Url = "url" };
            var result = await service.CreateModuleGroup(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task CreateModuleGroup_ShouldReturnFailure_WhenGroupExists()
        {
            _moduleGroupQueryRepo.GetBy(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, bool>>>()).Returns(new List<ModuleGroup> { new ModuleGroup() });
            var service = CreateService();
            var dto = new CreateModuleGroupDTO { Name = "Group", Description = "desc", Url = "url" };
            var result = await service.CreateModuleGroup(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleGroupExist, result.Message);
        }

        [Fact]
        public async Task DeleteModule_ShouldReturnSuccess_WhenModuleExists()
        {
            _moduleQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<Module, bool>>>()).Returns(new Module());
            _moduleCommandRepo.Delete(Arg.Any<Module>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _moduleGroupCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.SaveAsync().Returns(1);

            var service = CreateService();
            var result = await service.DeleteModule(1, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task DeleteModule_ShouldReturnFailure_WhenModuleDoesNotExist()
        {
            _moduleQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<Module, bool>>>()).Returns((Module)null);
            var service = CreateService();
            var result = await service.DeleteModule(1, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleNotExist, result.Message);
        }

        [Fact]
        public async Task EditModule_ShouldReturnFailure_WhenAllFieldsAreEmpty()
        {
            var service = CreateService();
            var dto = new EditModuleDTO { Id = 1, Name = "", Description = "", Url = "" };
            var result = await service.EditModule(1, dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleUpdateValidation, result.Message);
        }

        [Fact]
        public async Task EditModule_ShouldReturnFailure_WhenModuleDoesNotExist()
        {
            var service = CreateService();
            var dto = new EditModuleDTO { Id = 1, Name = "New", Description = "desc", Url = "url" };
            _moduleQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<Module, bool>>>()).Returns((Module)null);
            var result = await service.EditModule(1, dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleNotExist, result.Message);
        }

        [Fact]
        public async Task EditModule_ShouldReturnSuccess_WhenModuleExistsAndFieldsAreValid()
        {
            var module = new Module { Id = 1, Name = "Old", Description = "old", Url = "old" };
            _moduleQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<Module, bool>>>()).Returns(module);
            _moduleCommandRepo.Update(Arg.Any<Module>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _moduleGroupCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.SaveAsync().Returns(1);

            var service = CreateService();
            var dto = new EditModuleDTO { Id = 1, Name = "New", Description = "desc", Url = "url" };
            var result = await service.EditModule(1, dto, _auditLog);
            Assert.True(result.Status);
            Assert.Equal(ResponseMessages.ModuleUpdated, result.Message);
        }


        [Fact]
        public async Task DeleteModuleGroup_ShouldReturnSuccess_WhenModuleGroupExists()
        {
            _moduleGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, bool>>>()).Returns(new ModuleGroup());
            _moduleGroupCommandRepo.Delete(Arg.Any<ModuleGroup>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _moduleGroupCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.SaveAsync().Returns(1);

            var service = CreateService();
            var result = await service.DeleteModuleGroup(1, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task DeleteModuleGroup_ShouldReturnFailure_WhenModuleGroupDoesNotExist()
        {
            _moduleGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, bool>>>()).Returns((ModuleGroup)null);
            var service = CreateService();
            var result = await service.DeleteModuleGroup(1, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleGroupNotExist, result.Message);
        }

        [Fact]
        public async Task EditModuleGroup_ShouldReturnFailure_WhenAllFieldsAreEmpty()
        {
            var service = CreateService();
            var dto = new EditModuleGroupDTO { Id = 1, Name = "", Description = "", Url = "" };
            var result = await service.EditModuleGroup(1, dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleGroupUpdateValidation, result.Message);
        }

        [Fact]
        public async Task EditModuleGroup_ShouldReturnFailure_WhenModuleGroupDoesNotExist()
        {
            var service = CreateService();
            var dto = new EditModuleGroupDTO { Id = 1, Name = "New", Description = "desc", Url = "url" };
            _moduleGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, bool>>>()).Returns((ModuleGroup)null);
            var result = await service.EditModuleGroup(1, dto, _auditLog);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.ModuleGroupNotExist, result.Message);
        }

        [Fact]
        public async Task EditModuleGroup_ShouldReturnSuccess_WhenModuleGroupExistsAndFieldsAreValid()
        {
            var moduleGroup = new ModuleGroup { Id = 1, Name = "Old", Description = "old", Url = "old" };
            _moduleGroupQueryRepo.GetByDefault(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, bool>>>()).Returns(moduleGroup);
            _moduleGroupCommandRepo.Update(Arg.Any<ModuleGroup>());
            _auditLogCommandRepo.Add(Arg.Any<AuditLog>());
            _moduleGroupCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.SaveAsync().Returns(1);

            var service = CreateService();
            var dto = new EditModuleGroupDTO { Id = 1, Name = "New", Description = "desc", Url = "url" };
            var result = await service.EditModuleGroup(1, dto, _auditLog);
            Assert.True(result.Status);
            Assert.Equal(ResponseMessages.ModuleGroupUpdated, result.Message);
        }

        [Fact]
        public async Task GetAllModule_ShouldReturnMappedModules()
        {
            var modules = new List<Module> { new Module { Id = 1, Name = "A" }, new Module { Id = 2, Name = "B" } };
            _moduleQueryRepo.GetAllAsync().Returns(modules);
            _mapper.Map<List<ModuleDTO>>(modules).Returns(new List<ModuleDTO> { new ModuleDTO(), new ModuleDTO() });

            var service = CreateService();
            var result = await service.GetAllModule();
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllModule_ShouldReturnEmptyList_WhenNoModulesExist()
        {
            _moduleQueryRepo.GetAllAsync().Returns(new List<Module>());
            _mapper.Map<List<ModuleDTO>>(Arg.Any<List<Module>>()).Returns(new List<ModuleDTO>());

            var service = CreateService();
            var result = await service.GetAllModule();
            Assert.True(result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllModuleGroup_ShouldReturnMappedModuleGroups()
        {
            var moduleGroups = new List<ModuleGroup> { new ModuleGroup { Id = 1, Name = "A" }, new ModuleGroup { Id = 2, Name = "B" } };
            _moduleGroupQueryRepo.GetAllIncluding(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, object>>>()).Returns(moduleGroups.AsQueryable());
            _mapper.Map<List<ModuleGroupDTO>>(Arg.Any<List<ModuleGroup>>()).Returns(new List<ModuleGroupDTO> { new ModuleGroupDTO(), new ModuleGroupDTO() });

            var service = CreateService();
            var result = await service.GetAllModuleGroup();
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllModuleGroup_ShouldReturnEmptyList_WhenNoModuleGroupsExist()
        {
            _moduleGroupQueryRepo.GetAllIncluding(Arg.Any<System.Linq.Expressions.Expression<Func<ModuleGroup, object>>>()).Returns(new List<ModuleGroup>().AsQueryable());
            _mapper.Map<List<ModuleGroupDTO>>(Arg.Any<List<ModuleGroup>>()).Returns(new List<ModuleGroupDTO>());

            var service = CreateService();
            var result = await service.GetAllModuleGroup();
            Assert.True(result.Status);
            Assert.Empty(result.Data);
        }
    }
}
