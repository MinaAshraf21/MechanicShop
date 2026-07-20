using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
  public RelocateWorkOrderCommandValidator()
  {
    RuleFor(w => w.NewSpot)
                          .IsInEnum()
                          .WithMessage("The spot must be a valid enum value [A, B, C, D].");

    RuleFor(w =>w.WorkOrderId)
                            .NotEmpty()
                            .WithErrorCode("WorkOrderId_Required")
                            .WithMessage("Work order Id is required.");
  
  RuleFor(w => w.NewStartAt)
                        .GreaterThan(DateTimeOffset.Now)
                        .WithMessage("New start date must be in the future.");
  }
}