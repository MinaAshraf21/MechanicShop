using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrdersStats;

public sealed class GetWorkOrdersStatsQueryHandler(
  IAppDbContext context
) : IRequestHandler<GetWorkOrdersStatsQuery, Result<TodayWorkOrdersStatsDto>>
{
  public async Task<Result<TodayWorkOrdersStatsDto>> Handle(GetWorkOrdersStatsQuery request, CancellationToken cancellationToken)
  {
    var start = request.TodayDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    var end = start.AddDays(1);

    var workOrders = context.WorkOrders
                                .Include(w => w.Vehicle)
                                .Include(w => w.Tasks)
                                  .ThenInclude(t => t.Parts)
                                .Include(w => w.Labor)
                                .Include(w => w.Invoice)
                                .AsNoTracking()
                                .Where(w => w.StartAtUtc >= start && w.StartAtUtc < end);

    var totalOrders = await workOrders.CountAsync(cancellationToken);
    if(totalOrders == 0)
    {
      return new TodayWorkOrdersStatsDto(request.TodayDate,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);
    }

    var stats = await workOrders.ToListAsync(cancellationToken);

    var scheduledOrders = stats.Count(w => w.State == State.Scheduled);
    var completedOrders = stats.Count(w => w.State == State.Completed);
    var cancelledOrders = stats.Count(w => w.State == State.Cancelled);
    var inProgressOrders = stats.Count(w => w.State == State.InProgress);

    var totalRevenue = stats.Sum(x => x.Invoice?.Total ?? 0);
    var totalPartsCost = stats.Where(w => w.Invoice != null).Sum(i => i.TotalPartsCost ?? 0);
    var totalLaborsCost = stats.Where(w => w.Invoice != null).Sum(i => i.TotalLaborCost ?? 0);
    var uniqueVehicles = stats.Select(x => x.VehicleId).Distinct().Count();
    var uniqueCustomers = stats.Select(x => x.Vehicle!.CustomerId).Distinct().Count();

    var netProfit = totalRevenue - totalPartsCost - totalLaborsCost;
    var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

    var completionRate = totalOrders > 0 ? (decimal)completedOrders / totalOrders * 100 : 0;
    var cancellationRate = totalOrders > 0 ? (decimal)cancelledOrders / totalOrders * 100 : 0;
    var averageRevenuePerOrder = totalOrders > 0 ? totalRevenue / totalOrders : 0;

    var ordersPerVehicle = uniqueVehicles > 0 ? (decimal)totalOrders / uniqueVehicles : 0;
    var partsCostRatio = totalRevenue > 0 ? (totalPartsCost / totalRevenue) * 100 : 0;
    var laborCostRatio = totalRevenue > 0 ? (totalLaborsCost / totalRevenue) * 100 : 0;


    return new TodayWorkOrdersStatsDto
    (
      request.TodayDate,
      totalOrders,
      cancelledOrders,
      scheduledOrders,
      inProgressOrders,
      completedOrders,
      totalRevenue,
      totalPartsCost,
      totalLaborsCost,
      netProfit,
      uniqueVehicles,
      uniqueCustomers,
      profitMargin,
      completionRate,
      averageRevenuePerOrder,
      ordersPerVehicle,
      partsCostRatio,
      laborCostRatio,
      cancellationRate
    );
  }
}