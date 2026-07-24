using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Scheduling.GetDailySchedule;

public sealed record GetDailyScheduleQuery(DateOnly ScheduleDate, Guid? LaborId, TimeZoneInfo TimeZoneInfo)
  : ICachedQuery<Result<ScheduleDto>>
{
    public string CacheKey => $"work-order:{ScheduleDate:yyyy-MM-dd}:labor={LaborId?.ToString() ?? "-"}";
    public string[] Tags => ["work-order"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}