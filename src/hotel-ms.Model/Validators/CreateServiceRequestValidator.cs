using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequestDTO>
    {
        public CreateServiceRequestValidator()
        {
            RuleFor(x => x.ReservationId).GreaterThan(0).WithMessage("ReservationId is required.");
            RuleFor(x => x.ServiceType).NotEmpty().WithMessage("ServiceType is required.")
                .MaximumLength(100).WithMessage("ServiceType cannot exceed 100 characters.");
            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}
