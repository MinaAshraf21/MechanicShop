using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandValidator : AbstractValidator<UpdateWorkOrderRepairTasksCommand>
{
  public UpdateWorkOrderRepairTasksCommandValidator()
  {
    RuleFor(w =>w.WorkOrderId)
                            .NotEmpty()
                            .WithErrorCode("WorkOrderId_Required")
                            .WithMessage("Work order Id is required.");
  
    RuleFor(w => w.RepairTasksIds)
                            .NotEmpty()
                            .WithErrorCode("RepairTasks_Required")
                            .WithMessage("At least one repair task is required.");
  }
}