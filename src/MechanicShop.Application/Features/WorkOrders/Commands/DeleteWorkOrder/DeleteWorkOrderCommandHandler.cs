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

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandHandler(
  IAppDbContext context,
  ILogger<DeleteWorkOrderCommandHandler> logger,
  HybridCache cache)
  : IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
{
  public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await context.WorkOrders
                                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

    if(workOrder is null)
    {
      logger.LogWarning("Work order with Id: {id} was not found for deletion", request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    if(workOrder.State != State.Scheduled)
    {
      logger.LogError(
          "Deletion failed: only 'Scheduled' or 'Confirmed' WorkOrders can be deleted. Current status: {Status}",
          workOrder.State);
      return WorkOrderErrors.Readonly;
    }

    context.WorkOrders.Remove(workOrder);
    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("work-order",cancellationToken);
    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    return Result.Deleted;
  }
}