using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrdersStats;

public sealed record GetWorkOrdersStatsQuery(DateOnly TodayDate) : IRequest<Result<TodayWorkOrdersStatsDto>>;
