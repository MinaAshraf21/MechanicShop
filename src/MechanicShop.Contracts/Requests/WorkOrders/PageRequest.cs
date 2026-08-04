
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class PageRequest
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 10;
  public string? SearchTerm { get; set; } = "";
  public string SortColumn { get; set; } = "createdAt";
  public string SortDirection { get; set; } = "asc";
  public WorkOrderState? State { get; set; } = null;
  public Guid? VehicleId { get; set; } = null;
  public Guid? LaborId { get; set; } = null;
  public DateTime? StartDateFrom { get; set; } = null;
  public DateTime? StartDateTo { get; set; } = null;
  public DateTime? EndDateFrom { get; set; } = null;
  public DateTime? EndDateTo { get; set; } = null;
  public Spot? Spot { get; set; } = null;
}