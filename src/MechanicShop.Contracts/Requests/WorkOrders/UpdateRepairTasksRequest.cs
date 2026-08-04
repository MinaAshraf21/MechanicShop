namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class UpdateRepairTasksRequest
{
  public List<Guid> RepairTasksIds { get; set; }
}