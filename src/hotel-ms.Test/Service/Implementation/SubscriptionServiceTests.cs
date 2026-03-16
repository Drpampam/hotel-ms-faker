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
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class SubscriptionServiceTests
    {
        private readonly IDBCommandRepository<SubscriptionPlan> _planCommandRepo = Substitute.For<IDBCommandRepository<SubscriptionPlan>>();
        private readonly IDBQueryRepository<SubscriptionPlan> _planQueryRepo = Substitute.For<IDBQueryRepository<SubscriptionPlan>>();
        private readonly IDBQueryRepository<Tenant> _tenantQueryRepo = Substitute.For<IDBQueryRepository<Tenant>>();
        private readonly IDBCommandRepository<Tenant> _tenantCommandRepo = Substitute.For<IDBCommandRepository<Tenant>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly AuditLog _auditLog = new() { PerformedBy = "tester" };

        private SubscriptionService CreateService() => new(
            _planCommandRepo,
            _planQueryRepo,
            _tenantQueryRepo,
            _tenantCommandRepo,
            _auditLogCommandRepo,
            _mapper);

        [Fact]
        public async Task CreateSubscriptionPlanAsync_ShouldReturnFailure_WhenPlanExists()
        {
            _planQueryRepo.GetByDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SubscriptionPlan, bool>>>()).Returns(new SubscriptionPlan());
            var service = CreateService();
            var dto = new CreateSubscriptionPlanDTO { Name = "Basic" };
            var result = await service.CreateSubscriptionPlanAsync(dto, _auditLog);
            Assert.False(result.Status);
            Assert.Contains(ResponseMessages.SubscriptionExist, result.Message);
        }

        [Fact]
        public async Task CreateSubscriptionPlanAsync_ShouldReturnSuccess_WhenPlanDoesNotExist()
        {
            _planQueryRepo.GetByDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SubscriptionPlan, bool>>>()).Returns((SubscriptionPlan)null);
            var planEntity = new SubscriptionPlan();
            _mapper.Map<SubscriptionPlan>(Arg.Any<CreateSubscriptionPlanDTO>()).Returns(planEntity);
            _planCommandRepo.AddAsync(planEntity).Returns(Task.FromResult<object>(planEntity));
            _planCommandRepo.SaveAsync().Returns(1);
            _auditLogCommandRepo.AddAsync(Arg.Any<AuditLog>()).Returns(callInfo => Task.FromResult<object>(callInfo.Arg<AuditLog>()));
            _auditLogCommandRepo.SaveAsync().Returns(1);
            var service = CreateService();
            var dto = new CreateSubscriptionPlanDTO { Name = "Basic" };
            var result = await service.CreateSubscriptionPlanAsync(dto, _auditLog);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task GetSubscriptionPlanByIdAsync_ShouldReturnFailure_WhenPlanNotFound()
        {
            _planQueryRepo.FindAsync(Arg.Any<long>()).Returns((SubscriptionPlan)null);
            var service = CreateService();
            var result = await service.GetSubscriptionPlanByIdAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseMessages.SubscriptionNotExist, result.Message);
        }

        [Fact]
        public async Task GetSubscriptionPlanByIdAsync_ShouldReturnSuccess_WhenPlanFound()
        {
            var plan = new SubscriptionPlan { Id = 1, Name = "Basic" };
            _planQueryRepo.FindAsync(Arg.Any<long>()).Returns(plan);
            _mapper.Map<SubscriptionPlanResponseDTO>(plan).Returns(new SubscriptionPlanResponseDTO());
            var service = CreateService();
            var result = await service.GetSubscriptionPlanByIdAsync(1);
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetAllSubscriptionPlansAsync_ShouldReturnMappedPlans()
        {
            var plans = new List<SubscriptionPlan> { new SubscriptionPlan { Id = 1 }, new SubscriptionPlan { Id = 2 } };
            _planQueryRepo.GetAllAsync().Returns(plans);
            _mapper.Map<List<SubscriptionPlanResponseDTO>>(plans).Returns(new List<SubscriptionPlanResponseDTO> { new SubscriptionPlanResponseDTO(), new SubscriptionPlanResponseDTO() });
            var service = CreateService();
            var result = await service.GetAllSubscriptionPlansAsync();
            Assert.True(result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllSubscriptionPlansAsync_ShouldReturnEmptyList_WhenNoPlansExist()
        {
            _planQueryRepo.GetAllAsync().Returns(new List<SubscriptionPlan>());
            _mapper.Map<List<SubscriptionPlanResponseDTO>>(Arg.Any<List<SubscriptionPlan>>()).Returns(new List<SubscriptionPlanResponseDTO>());
            var service = CreateService();
            var result = await service.GetAllSubscriptionPlansAsync();
            Assert.True(result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_ShouldReturnFailure_WhenPlanNotFound()
        {
            _planQueryRepo.FindAsync(Arg.Any<long>()).Returns((SubscriptionPlan)null);
            var service = CreateService();
            var result = await service.DeleteSubscriptionPlanAsync(1, _auditLog);
            Assert.False(result.Status);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_ShouldReturnSuccess_WhenPlanFound()
        {
            var plan = new SubscriptionPlan { Id = 1, Name = "Basic" };
            _planQueryRepo.FindAsync(Arg.Any<long>()).Returns(plan);
            _planCommandRepo.UpdateAsync(plan).Returns(Task.CompletedTask);
            _auditLogCommandRepo.AddAsync(Arg.Any<AuditLog>()).Returns(callInfo => Task.FromResult<object>(callInfo.Arg<AuditLog>()));
            _auditLogCommandRepo.SaveAsync().Returns(1);
            var service = CreateService();
            var result = await service.DeleteSubscriptionPlanAsync(1, _auditLog);
            Assert.True(result.Status);
        }
    }
}
