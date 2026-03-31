using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class CreateDiscountRequestValidator : AbstractValidator<CreateDiscountRequestDTO>
    {
        public CreateDiscountRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Discount name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
            RuleFor(x => x.Percentage).InclusiveBetween(0, 100).WithMessage("Percentage must be between 0 and 100.");
            RuleFor(x => x.FixedAmount).GreaterThan(0).When(x => x.FixedAmount.HasValue)
                .WithMessage("FixedAmount must be greater than zero.");
            RuleFor(x => x).Must(x => x.Percentage > 0 || x.FixedAmount.HasValue)
                .WithMessage("Either Percentage or FixedAmount must be specified.");
            RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("EndDate must be after StartDate.");
        }
    }
}
