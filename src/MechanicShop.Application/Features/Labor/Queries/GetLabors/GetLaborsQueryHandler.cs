using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.Labor.Queries.GetLabors;

public sealed class GetLaborsQueryHandler(IAppDbContext context)
  : IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
  public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
  {
    var labors = await context.Employees
                        .AsNoTracking()
                        .Where(l => l.Role == Role.Labor)
                        .ToListAsync(cancellationToken);

    return labors.ToDtos();
  }
}