using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class AddRoomRequestValidator : AbstractValidator<AddRoomRequestDTO>
    {
        public AddRoomRequestValidator()
        {
            RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("PropertyId is required.");
            RuleFor(x => x.Number).NotEmpty().WithMessage("Room number is required.");
            RuleFor(x => x.Type).NotEmpty().WithMessage("Room type is required.");
            RuleFor(x => x.Capacity).InclusiveBetween(1, 100).WithMessage("Capacity must be between 1 and 100.");
            RuleFor(x => x.PricePerNight).GreaterThan(0).WithMessage("PricePerNight must be greater than zero.");
        }
    }
}
