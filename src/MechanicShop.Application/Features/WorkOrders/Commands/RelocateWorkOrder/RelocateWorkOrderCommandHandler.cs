using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;


public sealed class RelocateWorkOrderCommandHandler(
  IAppDbContext context,
  ILogger<RelocateWorkOrderCommandHandler> logger,
  HybridCache cache,
  IWorkOrderPolicy workOrderPolicy
) : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
  public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await context.WorkOrders
                                    .Include(w => w.Vehicle)
                                    .Include(w => w.Tasks)
                                    .Include(w => w.Labor)
                                    .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId,cancellationToken);

    if(workOrder is null)
    {
      logger.LogError("Work order with Id: {id} was not found for relocation", request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    // var duration = TimeSpan.FromMinutes(workOrder.Tasks.Sum(t => (int)t.EstimatedDuration));
    var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();
    var endAt = request.NewStartAt.Add(duration);
    if(workOrderPolicy.IsOutsideOperatingHours(request.NewStartAt, duration))
    {
      logger.LogError("The Work Order time ({StartAt} ? {EndAt}) is outside of store operating hours.", request.NewStartAt, endAt);
      return ApplicationErrors.WorkOrderOutsideOperatingHours(request.NewStartAt, endAt);
    }

    var checkSpotAvailability = await workOrderPolicy.CheckSpotAvailabilityAsync(request.NewSpot, request.NewStartAt, endAt, workOrder.Id, cancellationToken);
    if (checkSpotAvailability.IsFailure)
    {
      logger.LogError("Spot: {Spot} is not available.", request.NewSpot.ToString());
      return checkSpotAvailability.Errors!;
    }

    var isVehicleAlreadyScheduled = await workOrderPolicy.IsVehicleAlreadyScheduled(workOrder.VehicleId, request.NewStartAt, endAt, workOrder.Id);
    if (isVehicleAlreadyScheduled)
    {
      logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", workOrder.VehicleId);
      return ApplicationErrors.VehicleSchedulingConflict;
    }

    var isLaborOccupied = await workOrderPolicy.IsLaborOccupied(workOrder.LaborId, workOrder.Id, request.NewStartAt, endAt);
    if (isLaborOccupied)
    {
      logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);
      return ApplicationErrors.LaborOccupied;
    }

    var updateSpotResult = workOrder.UpdateSpot(request.NewSpot);
    if (updateSpotResult.IsFailure)
    {
      logger.LogError("Failed to update spot: {Error}", updateSpotResult.TopError.Description);
      return updateSpotResult.Errors!;
    }

    var updateTimingResult = workOrder.UpdateTiming(request.NewStartAt, endAt);
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