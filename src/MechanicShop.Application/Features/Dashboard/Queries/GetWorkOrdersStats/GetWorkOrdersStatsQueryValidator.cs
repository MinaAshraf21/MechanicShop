using FluentValidation;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrdersStats;

public sealed class GetWorkOrdersStatsQueryValidator : AbstractValidator<GetWorkOrdersStatsQuery>
{
  public GetWorkOrdersStatsQueryValidator()
  {
    RuleFor(w => w.TodayDate)
                .NotEmpty()
                .WithErrorCode("Date_Is_Required")
                .WithMessage("Date is required.");
  }
}