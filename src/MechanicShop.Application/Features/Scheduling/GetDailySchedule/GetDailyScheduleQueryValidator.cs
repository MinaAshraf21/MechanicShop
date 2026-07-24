using FluentValidation;

namespace MechanicShop.Application.Features.Scheduling.GetDailySchedule;

public sealed class GetDailyScheduleQueryValidator : AbstractValidator<GetDailyScheduleQuery>
{
  public GetDailyScheduleQueryValidator()
  {
    RuleFor(x => x.ScheduleDate)
                        .NotEmpty()
                        .WithErrorCode("ScheduleDate_Required")
                        .WithMessage("Schedule date is required.");
  }
}