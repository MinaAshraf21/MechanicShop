using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdatePartCommandValidator : AbstractValidator<UpdatePartCommand>
{
  public UpdatePartCommandValidator()
  {
    RuleFor(p => p.Name)
          .NotEmpty().WithMessage("Part name is required.")
          .MaximumLength(50).WithMessage("Part name length cannot exceed 50 chars.");

    RuleFor(p => p.Cost)
        .NotEmpty().WithMessage("Part cost is required.")
        .GreaterThan(0).WithMessage("Part cost must be greater than 0.");

    RuleFor(p => p.Quantity)
        .NotEmpty().WithMessage("Part quantity is required.")
        .GreaterThan(0).WithMessage("Part quantity must be greater than 0.");
  }
}