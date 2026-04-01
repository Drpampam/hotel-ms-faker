using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation
{
    public class DiscountService : IDiscountService
    {
        private readonly IDBCommandRepository<Discount> _discountCommandRepository;
        private readonly IDBQueryRepository<Discount> _discountQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;

        public DiscountService(
            IDBCommandRepository<Discount> discountCommandRepository,
            IDBQueryRepository<Discount> discountQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IMapper mapper,
            IUtility utility)
        {
            _discountCommandRepository = discountCommandRepository;
            _discountQueryRepository = discountQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _mapper = mapper;
            _utility = utility;
        }

        public async Task<BaseResponse<DiscountResponseDTO>> CreateDiscountAsync(CreateDiscountRequestDTO request, AuditLog auditLog)
        {
            var exists = await _discountQueryRepository.IsExistAsync(d =>
                d.Name == request.Name && !d.IsDeleted &&
                (!request.TenantId.HasValue || d.TenantId == request.TenantId));

            if (exists)
                return BaseResponse<DiscountResponseDTO>.Failure(new DiscountResponseDTO(), ResponseMessages.DiscountExists, ResponseStatusCode.DiscountExists);

            var discount = _mapper.Map<Discount>(request);
            discount.IsActive = true;
            discount.CreatedBy = auditLog.PerformedBy;
            discount.CreationDate = DateTime.UtcNow;

            _discountCommandRepository.Add(discount);
            _auditLogCommandRepository.Add(auditLog);
            await _discountCommandRepository.SaveAsync();

            var response = _mapper.Map<DiscountResponseDTO>(discount);
            return BaseResponse<DiscountResponseDTO>.Success(response, ResponseMessages.DiscountCreated, ResponseStatusCode.DiscountCreated);
        }

        public async Task<BaseResponse<DiscountResponseDTO>> UpdateDiscountAsync(UpdateDiscountRequestDTO request, AuditLog auditLog)
        {
            var discount = await _discountQueryRepository.FindAsync(request.Id);
            if (discount == null)
                return BaseResponse<DiscountResponseDTO>.Failure(new DiscountResponseDTO(), ResponseMessages.DiscountNotFound, ResponseStatusCode.DiscountNotFound);

            if (request.Name != null) discount.Name = request.Name;
            if (request.Description != null) discount.Description = request.Description;
            if (request.Percentage.HasValue) discount.Percentage = request.Percentage.Value;
            if (request.FixedAmount.HasValue) discount.FixedAmount = request.FixedAmount;
            if (request.StartDate.HasValue) discount.StartDate = request.StartDate;
            if (request.EndDate.HasValue) discount.EndDate = request.EndDate;
            if (request.MinimumStayDays.HasValue) discount.MinimumStayDays = request.MinimumStayDays;
            if (request.MaximumStayDays.HasValue) discount.MaximumStayDays = request.MaximumStayDays;
            if (request.IsActive.HasValue) discount.IsActive = request.IsActive.Value;
            discount.ModifiedBy = auditLog.PerformedBy;
            discount.LastModifiedDate = DateTime.UtcNow;

            await _discountCommandRepository.UpdateAsync(discount);
            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            var response = _mapper.Map<DiscountResponseDTO>(discount);
            return BaseResponse<DiscountResponseDTO>.Success(response, ResponseMessages.DiscountUpdated, ResponseStatusCode.DiscountUpdated);
        }

        public async Task<BaseResponse<DiscountResponseDTO>> GetDiscountByIdAsync(long discountId)
        {
            var discount = await _discountQueryRepository.FindAsync(discountId);
            if (discount == null)
                return BaseResponse<DiscountResponseDTO>.Failure(new DiscountResponseDTO(), ResponseMessages.DiscountNotFound, ResponseStatusCode.DiscountNotFound);

            var response = _mapper.Map<DiscountResponseDTO>(discount);
            return BaseResponse<DiscountResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<DiscountResponseDTO>>> GetDiscountsAsync(GetDiscountsInputDTO input)
        {
            var all = await _discountQueryRepository.GetByAsync(d =>
                !d.IsDeleted &&
                (!input.TenantId.HasValue || d.TenantId == input.TenantId.Value) &&
                (!input.PropertyId.HasValue || d.PropertyId == input.PropertyId.Value) &&
                (!input.IsActive.HasValue || d.IsActive == input.IsActive.Value));

            var paginated = _utility.Paginate(all, input.PageNumber, input.PageSize);
            var response = _mapper.Map<List<DiscountResponseDTO>>(paginated);

            return PageBaseResponse<List<DiscountResponseDTO>>.Success(response, ResponseMessages.DiscountsRetrieved,
                count: response.Count, totalPageCount: all.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }

        public async Task<BaseResponse> DeleteDiscountAsync(long discountId, AuditLog auditLog)
        {
            var discount = await _discountQueryRepository.FindAsync(discountId);
            if (discount == null)
                return BaseResponse.Failure(ResponseMessages.DiscountNotFound, ResponseStatusCode.DiscountNotFound);

            discount.IsDeleted = true;
            discount.ModifiedBy = auditLog.PerformedBy;
            discount.LastModifiedDate = DateTime.UtcNow;

            await _discountCommandRepository.UpdateAsync(discount);
            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            return BaseResponse.Success(ResponseMessages.DiscountDeleted, ResponseStatusCode.DiscountDeleted);
        }
    }
}
