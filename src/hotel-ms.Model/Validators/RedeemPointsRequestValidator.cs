using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class RedeemPointsRequestValidator : AbstractValidator<RedeemPointsRequestDTO>
    {
        public RedeemPointsRequestValidator()
        {
            RuleFor(x => x.Points).GreaterThan(0).WithMessage("Points must be greater than zero.");
            RuleFor(x => x.ReservationId).GreaterThan(0).WithMessage("ReservationId is required.");
        }
    }
}
