using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;

public sealed class AssignLaborCommandHandler(
  IAppDbContext context,
  ILogger<AssignLaborCommandHandler> logger,
  IWorkOrderPolicy policy,
  HybridCache cache
) : IRequestHandler<AssignLaborCommand, Result<Updated>>
{
  public async Task<Result<Updated>> Handle(AssignLaborCommand request, CancellationToken cancellationToken)
  {
    var labor = await context.Employees
                        .Where(e => e.Role == Role.Labor)
                        .FirstOrDefaultAsync(l => l.Id == request.LaborId, cancellationToken);

    var workOrder = await context.WorkOrders
                        .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

    if(labor is null)
    {
      logger.LogError("Labor with Id: {id} was not found.", request.LaborId);
      return ApplicationErrors.LaborNotFound;
    }
    if(workOrder is null)
    {
      logger.LogError("work order with Id: {id} was not found.", request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    if(await policy.IsLaborOccupied(request.LaborId, request.WorkOrderId, workOrder.StartAtUtc, workOrder.EndAtUtc))
    {
      logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);
      return ApplicationErrors.LaborOccupied;
    }

    var updateLaborResult = workOrder.UpdateLabor(request.LaborId);
    if (updateLaborResult.IsFailure)
    {
      foreach (var error in updateLaborResult.Errors!)
      {
        logger.LogError("[Update Labor]: {errorCode} - {errorDesc}", error.Code, error.Description);
      }
      return updateLaborResult.Errors!;
    }
    
    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("work-order", cancellationToken);
    return Result.Updated;
  }
}