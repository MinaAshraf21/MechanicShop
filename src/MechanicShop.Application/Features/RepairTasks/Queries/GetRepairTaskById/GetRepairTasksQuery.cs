using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public sealed record GetRepairTaskByIdQuery(Guid Id) : ICachedQuery<Result<RepairTaskDto>>
{
  public string CacheKey => $"repair-task_{Id}";

  public string[] Tags => ["repair-task"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}