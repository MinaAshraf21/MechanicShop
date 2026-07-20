using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandValidator : AbstractValidator<DeleteWorkOrderCommand>
{
  public DeleteWorkOrderCommandValidator()
  {
    RuleFor(w => w.WorkOrderId)
                        .NotEmpty()
                        .WithErrorCode("WorkOrderId_Required")
                        .WithMessage("Work order Id is required.");
  }
}