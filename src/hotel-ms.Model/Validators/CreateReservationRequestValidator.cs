using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequestDTO>
    {
        public CreateReservationRequestValidator()
        {
            RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("RoomId is required.");
            RuleFor(x => x.GuestId).GreaterThan(0).WithMessage("GuestId is required.");
            RuleFor(x => x.CheckInDate).NotEmpty().WithMessage("CheckInDate is required.")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("CheckInDate cannot be in the past.");
            RuleFor(x => x.CheckOutDate).NotEmpty().WithMessage("CheckOutDate is required.")
                .GreaterThan(x => x.CheckInDate).WithMessage("CheckOutDate must be after CheckInDate.");
            RuleFor(x => x.SpecialRequests).MaximumLength(500).WithMessage("SpecialRequests cannot exceed 500 characters.");
        }
    }
}
