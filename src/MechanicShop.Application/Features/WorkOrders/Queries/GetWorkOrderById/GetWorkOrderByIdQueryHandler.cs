using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryHandler(
  IAppDbContext context,
  ILogger<GetWorkOrderByIdQueryHandler> logger
) : IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
  public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
  {
    var workOrder = await context.WorkOrders
                                  .Include(w => w.Labor)
                                  .Include(w => w.Vehicle)
                                    .ThenInclude(v => v!.Customer)
                                  .Include(w => w.Tasks)
                                    .ThenInclude(t => t.Parts)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

    if(workOrder is null)
    {
      logger.LogError("Work order with Id: {id} was not found.", request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    return workOrder.ToDto();
  }
}