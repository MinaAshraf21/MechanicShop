using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Labor.Queries.GetLabors;

public sealed record GetLaborsQuery : ICachedQuery<Result<List<LaborDto>>>
{
  public string CacheKey => "labors";

  public string[] Tags => ["labor"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}