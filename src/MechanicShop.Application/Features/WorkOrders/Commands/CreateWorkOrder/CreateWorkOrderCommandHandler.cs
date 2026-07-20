using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler(
  IAppDbContext context,
  HybridCache cache,
  ILogger<CreateWorkOrderCommandHandler> logger,
  IWorkOrderPolicy policy
) : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
  public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
  {
    var vehicle = await context.Vehicles
                          .AsNoTracking()
                          .FirstOrDefaultAsync(v => v.Id == request.VehicleId);
    if(vehicle is null)
    {
      logger.LogWarning("Vehicle with Id: {id} was not found", request.VehicleId);
      return ApplicationErrors.VehicleNotFound;
    }

    var labor = await context.Employees
                          .AsNoTracking()
                          .Where(e => e.Role == Role.Labor)
                          .FirstOrDefaultAsync(v => v.Id == request.VehicleId);
    if(labor is null)
    {
      logger.LogWarning("Labor with Id: {id} was not found", request.laborId);
      return ApplicationErrors.VehicleNotFound;
    }

    var repairTasks = await context.RepairTasks
                          .Include(t => t.Parts)
                          .Where(t => request.RepairTaskIds.Contains(t.Id))
                          .ToListAsync(cancellationToken);

    if(repairTasks.Count != request.RepairTaskIds.Count)
    {
      var missingIds = request.RepairTaskIds.Except(repairTasks.Select(t => t.Id)).ToArray();

      logger.LogError("Some RepairTaskIds not found: {MissingIds}", string.Join(", ", missingIds));

      return ApplicationErrors.RepairTaskNotFound;
    }

    var duration = TimeSpan.FromMinutes(repairTasks.Sum(t => (int)t.EstimatedDuration));
    var endAt = request.startAt.Add(duration);
    if(policy.IsOutsideOperatingHours(request.startAt, duration))
    {
      logger.LogError("The WorkOrder time ({StartAt} ? {EndAt}) is outside of store operating hours.", request.startAt, endAt);
      return ApplicationErrors.WorkOrderOutsideOperatingHours(request.startAt, endAt);
    }

    var checkMinRequirementResult = policy.ValidateMinimumRequirement(request.startAt, endAt);
    if (checkMinRequirementResult.IsFailure)
    {
      logger.LogError("WorkOrder duration is shorter than the configured minimum.");
      return checkMinRequirementResult.Errors!;
    }

    var spotAvailabilityResult = await policy.CheckSpotAvailabilityAsync(request.Spot, request.startAt, endAt, null, cancellationToken);
    if (spotAvailabilityResult.IsFailure)
    {
      logger.LogError("Spot: {Spot} is not available.", request.Spot.ToString());
      return spotAvailabilityResult.Errors!;
    }

    bool isLaborOccupied = await policy.IsLaborOccupied(labor.Id, Guid.Empty, request.startAt, endAt);
    if (isLaborOccupied)
    {
      logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", request.laborId);
      return ApplicationErrors.LaborOccupied;
    }

    bool isVehicleAlreadyScheduled = await policy.IsVehicleAlreadyScheduled(labor.Id, request.startAt, endAt);
    if (isLaborOccupied)
    {
      logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", request.VehicleId);
      return ApplicationErrors.VehicleSchedulingConflict;
    }

    var createWorkOrderResult = WorkOrder.Create(Guid.NewGuid(), request.VehicleId, request.startAt, endAt, request.laborId!.Value, request.Spot, repairTasks);
  
    if(createWorkOrderResult.IsFailure)
      return createWorkOrderResult.Errors!;

    var workOrder = createWorkOrderResult.Value;
    workOrder.Vehicle = vehicle;
    workOrder.Labor = labor;

    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    context.WorkOrders.Add(workOrder);
    await context.SaveChangesAsync(cancellationToken);
    logger.LogInformation("WorkOrder with Id '{WorkOrderId}' created successfully.", workOrder.Id);

    await cache.RemoveByTagAsync("work-order", cancellationToken);

    return workOrder.ToDto();
  }
}