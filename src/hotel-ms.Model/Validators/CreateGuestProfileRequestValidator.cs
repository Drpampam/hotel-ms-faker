using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class CreateGuestProfileRequestValidator : AbstractValidator<CreateGuestProfileRequestDTO>
    {
        public CreateGuestProfileRequestValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("UserId is required.");
            RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("TenantId is required.");
            RuleFor(x => x.PassportNumber).MaximumLength(50).WithMessage("PassportNumber cannot exceed 50 characters.");
            RuleFor(x => x.Nationality).MaximumLength(50).WithMessage("Nationality cannot exceed 50 characters.");
            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow.AddYears(-18)).When(x => x.DateOfBirth.HasValue)
                .WithMessage("Guest must be at least 18 years old.");
            RuleFor(x => x.SpecialRequests).MaximumLength(500).WithMessage("SpecialRequests cannot exceed 500 characters.");
        }
    }
}
