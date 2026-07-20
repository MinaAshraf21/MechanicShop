using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;

public sealed class AssignLaborCommandValidator : AbstractValidator<AssignLaborCommand>
{
  public AssignLaborCommandValidator()
  {
    RuleFor(l => l.LaborId).NotEmpty().WithMessage("Labor id is required.");
    RuleFor(l => l.WorkOrderId).NotEmpty().WithMessage("Work order id is required.");
  }
}