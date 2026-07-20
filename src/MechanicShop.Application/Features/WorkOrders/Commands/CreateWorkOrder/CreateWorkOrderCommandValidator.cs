using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
  public CreateWorkOrderCommandValidator()
  {
    RuleFor(w => w.VehicleId).NotEmpty();

    RuleFor(w => w.Spot).IsInEnum().WithErrorCode("Spot_Invalid").WithMessage("Spot must be a valid Spot value. [A, B, C, D]");

    RuleFor(w => w.laborId).NotEmpty().Must(laborId => laborId is null || laborId != Guid.Empty)
            .WithMessage("If provided, LaborId must not be empty.");;

    RuleFor(w => w.startAt).GreaterThan(DateTimeOffset.Now).WithMessage("Start date must be in the future.");

    RuleFor(w => w.RepairTaskIds).NotEmpty().WithMessage("At least one task is required.");
  }
}