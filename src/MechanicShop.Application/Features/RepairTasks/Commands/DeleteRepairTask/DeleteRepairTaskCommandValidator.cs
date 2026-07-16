using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.DeleteRepairTask;

public sealed class DeleteRepairTaskCommandValidator : AbstractValidator<DeleteRepairTaskCommand>
{
  public DeleteRepairTaskCommandValidator()
  {
    RuleFor(r => r.RepairTaskId).NotEmpty().WithMessage("Repair task id is required.");
  }
}