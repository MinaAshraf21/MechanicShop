using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;


public sealed class UpdateWorkOrderRepairTasksCommandHandler(
  IAppDbContext context,
  ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
  HybridCache cache,
  IWorkOrderPolicy workOrderPolicy) : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
  public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await context.WorkOrders
                                    .Include(w => w.Tasks)
                                    .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

    if(workOrder is null)
    {
      logger.LogError("Work order with Id: {id} was not found for relocation", request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    var repairTasks = await context.RepairTasks
                                  .Where(t => request.RepairTasksIds.Contains(t.Id))
                                  .ToListAsync(cancellationToken);

    if(repairTasks.Count != request.RepairTasksIds.Count)
    {
      var missingIds = repairTasks.Select(t => t.Id).ToArray();
      logger.LogError("One or more RepairTasks not found. {ids}", string.Join(", ", missingIds));
      return ApplicationErrors.RepairTaskNotFound;
    }

    var clearRepairTasksResult =  workOrder.ClearRepairTasks();
    if(clearRepairTasksResult.IsFailure)
      return clearRepairTasksResult.Errors!;

    foreach (var task in repairTasks)
    {
      var addRepairTaskResult = workOrder.AddRepairTask(task);
      if (addRepairTaskResult.IsFailure)
      {
        return addRepairTaskResult.Errors!;
      }
    }

    var duration = TimeSpan.FromMinutes(repairTasks.Sum(t => (int)t.EstimatedDuration));
    var endAt = workOrder.StartAtUtc.Add(duration);

    if(workOrderPolicy.IsOutsideOperatingHours(workOrder.StartAtUtc, duration))
    {
      logger.LogError("The Work Order time ({StartAt} ? {EndAt}) is outside of store operating hours.", workOrder.StartAtUtc, endAt);
      return ApplicationErrors.WorkOrderOutsideOperatingHours(workOrder.StartAtUtc, endAt);
    }

    var checkSpotAvailability = await workOrderPolicy.CheckSpotAvailabilityAsync(workOrder.Spot, workOrder.StartAtUtc, endAt, workOrder.Id, cancellationToken);
    if (checkSpotAvailability.IsFailure)
    {
      logger.LogError("Spot: {Spot} is not available.", workOrder.Spot.ToString());
      return checkSpotAvailability.Errors!;
    }


    var isLaborOccupied = await workOrderPolicy.IsLaborOccupied(workOrder.LaborId, workOrder.Id, workOrder.StartAtUtc, endAt);
    if (isLaborOccupied)
    {
      logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);
      return ApplicationErrors.LaborOccupied;
    }

    var updateTimingResult = workOrder.UpdateTiming(workOrder.StartAtUtc, endAt);
    if (updateTimingResult.IsFailure)
    {
      logger.LogError("Failed to update timing: {Error}", updateTimingResult.TopError.Description);
      return updateTimingResult.Errors!;
    }

    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("work-order", cancellationToken);
    workOrder.AddDomainEvent(new WorkOrderCollectionModified());
    logger.LogInformation("Work order with Id: {id}, relocated successfully.", workOrder.Id);

    return Result.Updated;
  }
}