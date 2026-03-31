using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class AccruePointsRequestValidator : AbstractValidator<AccruePointsRequestDTO>
    {
        public AccruePointsRequestValidator()
        {
            RuleFor(x => x.Points).GreaterThan(0).WithMessage("Points must be greater than zero.");
            RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters.");
        }
    }
}
