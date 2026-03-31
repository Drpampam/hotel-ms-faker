using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequestDTO>
    {
        public CreatePaymentRequestValidator()
        {
            RuleFor(x => x.ReservationId).GreaterThan(0).WithMessage("ReservationId is required.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
            RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("PaymentMethod is required.");
        }
    }
}
