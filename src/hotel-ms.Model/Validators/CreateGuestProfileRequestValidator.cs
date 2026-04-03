using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class CreateGuestProfileRequestValidator : AbstractValidator<CreateGuestProfileRequestDTO>
    {
        public CreateGuestProfileRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PassportNumber)
                .MaximumLength(100).WithMessage("Passport number cannot exceed 100 characters.");

            RuleFor(x => x.Nationality)
                .MaximumLength(100).WithMessage("Nationality cannot exceed 100 characters.");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow.AddYears(-18)).When(x => x.DateOfBirth.HasValue)
                .WithMessage("Guest must be at least 18 years old.");

            RuleFor(x => x.SpecialRequests)
                .MaximumLength(500).WithMessage("Special requests cannot exceed 500 characters.");
        }
    }
}
