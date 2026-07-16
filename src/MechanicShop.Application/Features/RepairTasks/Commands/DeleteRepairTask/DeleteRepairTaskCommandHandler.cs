using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.DeleteRepairTask;

public sealed class DeleteRepairTaskCommandHandler(
  IAppDbContext context,
  ILogger<DeleteRepairTaskCommandHandler> logger,
  HybridCache cache
) : IRequestHandler<DeleteRepairTaskCommand, Result<Deleted>>
{
  public async Task<Result<Deleted>> Handle(DeleteRepairTaskCommand request, CancellationToken cancellationToken)
  {
    var repairTask = await context.RepairTasks
                            .FirstOrDefaultAsync(r => r.Id == request.RepairTaskId, cancellationToken);

    if(repairTask is null)
    {
      logger.LogWarning("Repair task with Id: {repairTaskId} was not found for deletion", request.RepairTaskId);
      return ApplicationErrors.RepairTaskNotFound;
    }

    bool taskInUse = await context.WorkOrders
                            .SelectMany(w => w.Tasks)
                            .AnyAsync(r => r.Id == request.RepairTaskId, cancellationToken);

    if (taskInUse)
    {
      logger.LogWarning("Repair task with Id: {repairTaskId} cannot be deleted. It's in use by work orders.", request.RepairTaskId);
      return RepairTaskErrors.InUse;
    }

    context.RepairTasks.Remove(repairTask);
    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("repair-task", cancellationToken);
    logger.LogInformation("RepairTask {RepairTaskId} deleted successfully.", request.RepairTaskId);

    return Result.Deleted;
  }
}