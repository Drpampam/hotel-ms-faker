using AutoMapper;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<CreateUserRequestDTO, ApplicationUser>()
                .ForMember(x => x.UserName, y => { y.MapFrom(p => p.Email); })
                .ForMember(x => x.FullName, y => { y.MapFrom(p => p.FullName); })
                .ForMember(x => x.Email, y => { y.MapFrom(p => p.Email); })
                .ForMember(x => x.EmailConfirmed, y => { y.MapFrom(p => true); })
                .ForMember(x => x.PhoneNumberConfirmed, y => { y.MapFrom(p => false); })
                .ForMember(x => x.TwoFactorEnabled, y => { y.MapFrom(p => false); })
                .ForMember(x => x.AccessFailedCount, y => { y.MapFrom(p => 0); })
                .ReverseMap();

            CreateMap<CreateUserRequestDTO, ApplicationRole>()
                .ForMember(x => x.Name, y => { y.MapFrom(p => p.Role.ToString()); });

            CreateMap<ApplicationUser, ApplicationUserDTO>().ReverseMap();
            CreateMap<ApplicationUserRole, RoleDTO>().ReverseMap();
            CreateMap<ModuleGroup, ModuleGroupDTO>();
            CreateMap<Module, ModuleDTO>();
            CreateMap<ApplicationRole, RoleDTO>();

            CreateMap<AddPropertyRequestDTO, Property>();
            CreateMap<CreateAddressRequestDTO, Address>();
            CreateMap<Address, AddressResponseDTO>();
            CreateMap<Property, PropertyResponseDTO>();
        }
    }
}
