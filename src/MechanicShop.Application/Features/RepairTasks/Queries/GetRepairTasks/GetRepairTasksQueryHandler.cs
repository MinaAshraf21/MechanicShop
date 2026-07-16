using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public sealed class GetRepairTasksQueryHandler(
  IAppDbContext context
) : IRequestHandler<GetRepairTasksQuery, Result<List<RepairTaskDto>>>
{
  public async Task<Result<List<RepairTaskDto>>> Handle(GetRepairTasksQuery request, CancellationToken cancellationToken)
  {
    var repairTasks = await context.RepairTasks
                              .Include(r => r.Parts)
                              .AsNoTracking()
                              .ToListAsync();

    return repairTasks.ToDtos();
  }
}