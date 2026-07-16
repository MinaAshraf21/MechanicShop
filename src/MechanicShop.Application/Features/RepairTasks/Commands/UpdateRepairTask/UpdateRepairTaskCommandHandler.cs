using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdateRepairTaskCommandHandler(
  IAppDbContext context,
  ILogger<UpdateRepairTaskCommandHandler> logger,
  HybridCache cache
) : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
  public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand request, CancellationToken cancellationToken)
  {
    var repairTask = await context.RepairTasks
                            .Include(r => r.Parts)
                            .FirstOrDefaultAsync(r => r.Id == request.RepairTaskId, cancellationToken);

    if(repairTask is null)
    {
      logger.LogWarning("Repair task with Id: {repairTaskId} was not found for update", request.RepairTaskId);
      return ApplicationErrors.RepairTaskNotFound;
    }

    var parts = new List<Part>();

    foreach (var p in request.Parts)
    {
      var createPartResult = Part.Create(p.Id ?? Guid.NewGuid(), p.Cost, p.Name, p.Quantity);
      if (createPartResult.IsFailure)
      {
        return createPartResult.Errors!;
      }
      parts.Add(createPartResult.Value);
    }

    var updateRepairTaskResult = repairTask.Update(request.Name, request.LaborCost, request.Duration);
    if (updateRepairTaskResult.IsFailure)
      return updateRepairTaskResult.Errors!;

    var updatePartsResult = repairTask.UpdateParts(parts);
    if (updatePartsResult.IsFailure)
      return updatePartsResult.Errors!;
    
    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("repair-task", cancellationToken);

    return Result.Updated;
  }
}