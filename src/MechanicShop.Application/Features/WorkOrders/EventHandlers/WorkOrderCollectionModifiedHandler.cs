using MechanicShop.Application.Abstractions;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class WorkOrderCollectionModifiedHandler(IWorkOrderNotifier workOrderNotifier) : INotificationHandler<WorkOrderCollectionModified>
{
  public async Task Handle(WorkOrderCollectionModified notification, CancellationToken cancellationToken)
  {
    await workOrderNotifier.NotifyWorkOrdersChangedAsync(cancellationToken);
  }
}