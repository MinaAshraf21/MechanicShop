using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
public sealed class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
{
  public GetWorkOrderByIdQueryValidator()
  {
    RuleFor(w =>w.WorkOrderId)
                            .NotEmpty()
                            .WithErrorCode("WorkOrderId_Required")
                            .WithMessage("Work order Id is required.");
  }
}