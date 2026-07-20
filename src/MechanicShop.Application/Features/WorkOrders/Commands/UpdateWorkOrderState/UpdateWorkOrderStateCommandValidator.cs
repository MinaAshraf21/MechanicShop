using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderStateCommand>
{
  public UpdateWorkOrderStateCommandValidator()
  {
    RuleFor(w =>w.WorkOrderId)
                            .NotEmpty()
                            .WithErrorCode("WorkOrderId_Required")
                            .WithMessage("Work order Id is required.");

    RuleFor(w => w.NewState)
                          .IsInEnum()
                          .WithErrorCode("WorkOrderStatus_Invalid")
                          .WithMessage("The state must be a valid state value.");
  }
}