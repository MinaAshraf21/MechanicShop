using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public sealed class GetRepairTaskByIdQueryHandler(
  IAppDbContext context,
  ILogger<GetRepairTaskByIdQueryHandler> logger
) : IRequestHandler<GetRepairTaskByIdQuery, Result<RepairTaskDto>>
{
  public async Task<Result<RepairTaskDto>> Handle(GetRepairTaskByIdQuery request, CancellationToken cancellationToken)
  {
    var repairTask = await context.RepairTasks
                              .Include(r => r.Parts)
                              .AsNoTracking()
                              .FirstOrDefaultAsync(r => r.Id == request.Id);

    if(repairTask is null)
    {
      logger.LogWarning("Repair task with Id: {id} was not found.", request.Id);
      return ApplicationErrors.RepairTaskNotFound;
    }

    return repairTask.ToDto();
  }
}