using System.Data;
using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandValidator : AbstractValidator<CreateRepairTaskCommand>
{
  public CreateRepairTaskCommandValidator()
  {
    RuleFor(r => r.Name)
          .NotEmpty().WithMessage("Repair task name is required.")
          .MaximumLength(50).WithMessage("Repair task name length cannot exceed 50 chars.");

    RuleFor(r => r.LaborCost)
        .NotEmpty().WithMessage("Labor cost is required.")
        .GreaterThan(0).WithMessage("Labor cost must be greater than 0.");

    RuleFor(r => r.Duration)
        .NotNull().WithMessage("Estimated duration is required.")
        .IsInEnum();

    RuleFor(r => r.Parts)
        .NotNull().WithMessage("Parts list cannot be null.")
        .Must(p => p.Count > 0).WithMessage("At least one part is required.");

    RuleForEach(r => r.Parts).SetValidator(new CreatePartCommandValidator());
  }
}