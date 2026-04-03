using FluentValidation;
using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Model.Validators
{
    public class CreateHousekeepingTaskValidator : AbstractValidator<CreateHousekeepingTaskDTO>
    {
        public CreateHousekeepingTaskValidator()
        {
            RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("RoomId is required.");
            RuleFor(x => x.TaskType).NotEmpty().WithMessage("TaskType is required.")
                .MaximumLength(100).WithMessage("TaskType cannot exceed 100 characters.");
            RuleFor(x => x.Priority).NotEmpty().WithMessage("Priority is required.")
                .Must(p => new[] { "Low", "Normal", "Medium", "High", "Urgent", "Critical" }.Contains(p))
                .WithMessage("Priority must be Low, Normal, High, or Urgent.");
            RuleFor(x => x.ScheduledAt).Must(d => !d.HasValue || d.Value > DateTime.MinValue)
                .WithMessage("ScheduledAt must be a valid date if provided.");
            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}
