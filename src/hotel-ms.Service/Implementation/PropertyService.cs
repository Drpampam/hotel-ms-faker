using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Identity;

namespace hotelier_core_app.Service.Implementation
{
    public class PropertyService : IPropertyService
    {
        private readonly IDBQueryRepository<Tenant> _tenantQueryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDBCommandRepository<Address> _addressCommandRepository;
        private readonly IDBCommandRepository<Property> _propertyCommandRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IDBQueryRepository<Property> _propertyQueryRepository;
        private readonly IDBQueryRepository<Address> _addressQueryRepository;
        private readonly IUtility _utility;

        public PropertyService(IDBQueryRepository<Tenant> tenantQueryRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IDBCommandRepository<Address> addressCommandRepository,
            IDBCommandRepository<Property> propertyCommandRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IDBQueryRepository<Property> propertyQueryRepository,
            IDBQueryRepository<Address> addressQueryRepository,
            IUtility utility)
        {
            _tenantQueryRepository = tenantQueryRepository;
            _userManager = userManager;
            _mapper = mapper;
            _addressCommandRepository = addressCommandRepository;
            _propertyCommandRepository = propertyCommandRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _propertyQueryRepository = propertyQueryRepository;
            _addressQueryRepository = addressQueryRepository;
            _utility = utility;
        }

        // add property
        public async Task<BaseResponse> AddProperty(AddPropertyRequestDTO request, AuditLog auditLog)
        {
            // validate input
            // verify that tenant exists
            // verify that user belongs to tenant
            // verify that user has permissions (via controller)

            var tenant = await _tenantQueryRepository.FindAsync(request.TenantId);
            if (tenant == null) return BaseResponse.Failure(ResponseMessages.TenantNotExisting);

            var currentUser = await _userManager.FindByEmailAsync(auditLog.PerformerEmail);
            if (currentUser == null) return BaseResponse.Failure(ResponseMessages.UserDoesNotExist);
            if (currentUser.TenantId != request.TenantId) return BaseResponse.Failure(ResponseMessages.UserNotInTenant);

            var address = _mapper.Map<Address>(request.Address);
            _addressCommandRepository.Add(address);

            var property = _mapper.Map<Property>(request);
            property.AddressId = address.Id;
            property.CreatedBy = auditLog.PerformedBy;
            property.CreationDate = DateTime.UtcNow;
            _propertyCommandRepository.Add(property);

            _auditLogCommandRepository.Add(auditLog);

            return BaseResponse.Success();
        }

        public async Task<BaseResponse> UpdateProperty(UpdatePropertyRequestDTO request, AuditLog auditLog)
        {
            var property = await _propertyQueryRepository.FindAsync(request.Id);
            if(property == null) return BaseResponse.Failure(ResponseMessages.PropertyNotFound);

            var currentUser = await _userManager.FindByEmailAsync(auditLog.PerformerEmail);
            if (currentUser == null) return BaseResponse.Failure(ResponseMessages.UserDoesNotExist);
            if (currentUser.TenantId != property.TenantId) return BaseResponse.Failure(ResponseMessages.UserNotInTenant);

            property.Name = request.Name;
            property.Description = request.Description;
            property.Image = request.Image;
            _propertyCommandRepository.Update(property);

            var address = _addressQueryRepository.Find(property.AddressId);
            address.Street = request.Address.Street;
            address.City = request.Address.City;
            address.State = request.Address.State;
            address.ZipCode = request.Address.ZipCode;
            address.Country = request.Address.Country;
            address.Latitude = request.Address.Latitude;
            address.Longitude = request.Address.Longitude;
            _addressCommandRepository.Update(address);

            return BaseResponse.Success();
        }

        public async Task<BaseResponse<PropertyResponseDTO>> GetById(long id)
        {
            var property = _propertyQueryRepository.FindByInclude(p => p.Id == id, p => p.Address);
            if (property == null) return BaseResponse<PropertyResponseDTO>.Failure(new PropertyResponseDTO(), ResponseMessages.PropertyNotFound);

            var response = _mapper.Map<PropertyResponseDTO>(property);
            return BaseResponse<PropertyResponseDTO>.Success(response);
        }

        public async Task<PageBaseResponse<List<PropertyResponseDTO>>> GetTenantPropertyList(GetPropertiesInputDTO input)
        {
            var properties = await _propertyQueryRepository.GetByAsync(p => p.TenantId == input.TenantId);
            var paginated = _utility.Paginate(properties, input.PageNumber, input.PageSize);
            var response = _mapper.Map<List<PropertyResponseDTO>>(paginated);

            return PageBaseResponse<List<PropertyResponseDTO>>.Success(response, count: response.Count(), totalPageCount: properties.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }
    }
}
