namespace MechanicShop.Application.Features.Dashboard.Dtos;

public sealed record TodayWorkOrdersStatsDto
(
  DateOnly TodayDate,
  int TotalOrders,
  int CancelledOrders,
  int ScheduledOrders,
  int InProgressOrders,
  int CompletedOrders,
  decimal TotalRevenue,
  decimal TotalPartsCost,
  decimal TotalLaborsCost,
  decimal NetProfit,
  int TotalVehicles,
  int TotalCustomers,
  decimal ProfitMargin,
  decimal CompletionRate,
  decimal AverageRevenuePerOrder,
  decimal OrdersPerVehicle,
  decimal PartsCostRatio,
  decimal LaborCostRatio,
  decimal CancellationRate
);