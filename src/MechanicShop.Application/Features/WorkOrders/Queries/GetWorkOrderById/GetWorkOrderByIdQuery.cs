using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;


public sealed record GetWorkOrderByIdQuery(Guid WorkOrderId) : ICachedQuery<Result<WorkOrderDto>>
{
  public string CacheKey => $"work-order_{WorkOrderId}";

  public string[] Tags => ["work-order"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}