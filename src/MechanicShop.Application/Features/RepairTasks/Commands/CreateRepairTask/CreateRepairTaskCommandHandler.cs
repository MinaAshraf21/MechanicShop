using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandHandler(
  IAppDbContext context,
  ILogger<CreateRepairTaskCommandHandler> logger,
  HybridCache cache
)
  : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
{
  public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand request, CancellationToken cancellationToken)
  {

    bool nameExists = await context.RepairTasks
                                    .AnyAsync(r => EF.Functions.Like(r.Name, request.Name), cancellationToken);

    if (nameExists)
    {
      logger.LogWarning("Duplicate part name '{PartName}'.", request.Name);

      return RepairTaskErrors.DuplicateName;
    }

    List<Part> parts = new();
    foreach (var p in request.Parts)
    {
      var createPartResult = Part.Create(Guid.NewGuid(), p.Cost, p.Name, p.Quantity);
      if (createPartResult.IsFailure)
      {
        return createPartResult.Errors!;
      }
      parts.Add(createPartResult.Value);
    }
    var repairTaskResult = RepairTask.Create(Guid.NewGuid(), request.Name, request.LaborCost, request.Duration, parts);
    if (repairTaskResult.IsFailure)
    {
      return repairTaskResult.Errors!;
    }

    var repairTask = repairTaskResult.Value;

    context.RepairTasks.Add(repairTask);
    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("repair-task", cancellationToken);

    return repairTask.ToDto();
  }
}