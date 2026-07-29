namespace MechanicShop.Application.Abstractions;

public interface IWorkOrderNotifier
{
  Task NotifyWorkOrdersChangedAsync(CancellationToken cancellationToken = default);
}