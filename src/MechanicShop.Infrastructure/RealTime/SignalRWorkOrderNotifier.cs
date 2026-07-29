using MechanicShop.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace MechanicShop.Infrastructure.RealTime;

public sealed class SignalRWorkOrderNotifier(IHubContext<WorkOrderHub> hubContext) : IWorkOrderNotifier
{
  public async Task NotifyWorkOrdersChangedAsync(CancellationToken cancellationToken = default)
  {
    await hubContext.Clients.All.SendAsync("WorkOrdersChanged", cancellationToken);
  }
}