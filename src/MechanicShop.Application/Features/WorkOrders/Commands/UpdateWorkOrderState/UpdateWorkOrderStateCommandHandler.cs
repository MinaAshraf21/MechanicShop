using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed class UpdateWorkOrderStateCommandHandler(
  IAppDbContext context,
  HybridCache cache,
  ILogger<UpdateWorkOrderStateCommandHandler> logger,
  TimeProvider timeProvider
  ) : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
  public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await context.WorkOrders
                                    .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

    if(workOrder is null)
    {
      logger.LogError("Work order with Id: {id} was not found for relocation", request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }
    
    if (workOrder.StartAtUtc > timeProvider.GetUtcNow())
    {
      logger.LogError("State transition for WorkOrder Id '{WorkOrderId}` is not allowed before the work order�s scheduled start time.", request.WorkOrderId);
      return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc);
    }

    var updateStateResult = workOrder.UpdateState(request.NewState);

    if (updateStateResult.IsFailure)
    {
      logger.LogError("Failed to update the status of work order {id}", workOrder.Id);
      return updateStateResult.Errors!;
    }

    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("work-order", cancellationToken);

    if(request.NewState == State.Completed)
      workOrder.AddDomainEvent(new WorkOrderCompleted{WorkOrderId = workOrder.Id});
    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    return Result.Updated;
  }
}