using System.Data;
using FluentValidation;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;

public sealed class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
  public GetRepairTaskByIdQueryValidator()
  {
    RuleFor(r => r.Id).NotEmpty()
                      .WithErrorCode("RepairTaskId_Is_Required")
                      .WithMessage("Repair task Id is required.");
  }
}