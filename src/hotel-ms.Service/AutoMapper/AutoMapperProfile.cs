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
            // User
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

            // Module
            CreateMap<ModuleGroup, ModuleGroupDTO>();
            CreateMap<Module, ModuleDTO>();
            CreateMap<ApplicationRole, RoleDTO>();

            // Property & Address
            CreateMap<AddPropertyRequestDTO, Property>();
            CreateMap<CreateAddressRequestDTO, Address>();
            CreateMap<Address, AddressResponseDTO>();
            CreateMap<Property, PropertyResponseDTO>();

            // Room
            CreateMap<AddRoomRequestDTO, Room>();
            CreateMap<Room, RoomResponseDTO>();

            // Payment
            CreateMap<CreatePaymentRequestDTO, Payment>();
            CreateMap<Payment, PaymentResponseDTO>();

            // Service Request
            CreateMap<CreateServiceRequestDTO, ServiceRequest>();
            CreateMap<ServiceRequest, ServiceRequestResponseDTO>();

            // Discount
            CreateMap<CreateDiscountRequestDTO, Discount>();
            CreateMap<Discount, DiscountResponseDTO>();

            // GuestProfile
            CreateMap<CreateGuestProfileRequestDTO, GuestProfile>();
            CreateMap<GuestProfile, GuestProfileResponseDTO>()
                .ForMember(x => x.FullName, y => y.Ignore())
                .ForMember(x => x.Email, y => y.Ignore())
                .ForMember(x => x.PhoneNumber, y => y.Ignore());

            // Reservation
            CreateMap<Reservation, ReservationResponseDTO>()
                .ForMember(x => x.RoomNumber, y => y.Ignore())
                .ForMember(x => x.RoomType, y => y.Ignore())
                .ForMember(x => x.GuestName, y => y.Ignore())
                .ForMember(x => x.GuestEmail, y => y.Ignore())
                .ForMember(x => x.NightsCount, y => y.MapFrom(r =>
                    (int)(r.CheckOutDate.Date - r.CheckInDate.Date).TotalDays));

            // Housekeeping
            CreateMap<HousekeepingTask, HousekeepingTaskResponseDTO>()
                .ForMember(x => x.AvailableTriggers, y => y.Ignore());

            // Billing
            CreateMap<Invoice, InvoiceResponseDTO>()
                .ForMember(x => x.LineItems, y => y.Ignore());
            CreateMap<InvoiceLineItem, InvoiceLineItemDTO>();

            // Loyalty
            CreateMap<LoyaltyProgram, LoyaltyResponseDTO>()
                .ForMember(x => x.PointsBalance, y => y.MapFrom(l => l.PointsEarned - l.PointsRedeemed))
                .ForMember(x => x.GuestName, y => y.Ignore())
                .ForMember(x => x.GuestEmail, y => y.Ignore())
                .ForMember(x => x.Tier, y => y.Ignore());

            // Audit Log
            CreateMap<AuditLog, AuditLogResponseDTO>();
        }
    }
}
