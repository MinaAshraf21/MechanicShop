using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class UpdateWorkOrderStateRequest
{
  public WorkOrderState State { get; set; }
}