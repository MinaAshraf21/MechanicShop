using System.Net;
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Scheduling.GetDailySchedule;

public sealed class GetDailyScheduleQueryHandler(
  IAppDbContext context,
  TimeProvider timeProvider
) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
  public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
  {
    var localStart = request.ScheduleDate.ToDateTime(TimeOnly.MinValue);
    var localEnd = localStart.AddDays(1);

    var startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, request.TimeZoneInfo);
    var endUtc = TimeZoneInfo.ConvertTimeToUtc(localEnd, request.TimeZoneInfo);

    var workOrders = await context.WorkOrders
                                    .Include(w => w.Vehicle)
                                    .Include(w => w.Labor)
                                    .Include(w => w.Tasks)
                                    .Where(w =>
                                            w.StartAtUtc < endUtc
                                            && w.EndAtUtc > startUtc
                                            && (request.LaborId != null ? w.LaborId == request.LaborId : true)
                                          )
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

    var now = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), request.TimeZoneInfo);

    var result = new ScheduleDto(request.ScheduleDate, localEnd < now, []);

    foreach (var spot in Enum.GetValues<Spot>())
    {
      var current = localStart;
      var slots = new List<AvailabilitySlotDto>();
      var workOrderBySlot = workOrders.Where(w => w.Spot == spot).OrderBy(w => w.StartAtUtc).ToList();
      while(current < localEnd)
      {
        var next = current.AddMinutes(15);
        var curUtc = TimeZoneInfo.ConvertTimeToUtc(current, request.TimeZoneInfo);
        var nextUtc = TimeZoneInfo.ConvertTimeToUtc(next, request.TimeZoneInfo);

        var wo = workOrderBySlot.FirstOrDefault(w => w.StartAtUtc < nextUtc && w.EndAtUtc > curUtc);
        if(wo != null)
        {
          if(!slots.Any(s => s.WorkOrderId == wo.Id))
          {
            slots.Add(new AvailabilitySlotDto
            {
              IsAvailable = false,
              Spot = spot,
              WorkOrderLocked = !wo.IsEditable,
              StartAtUtc = curUtc,
              EndAtUtc = nextUtc,
              Vehicle = $"{wo.Vehicle!.Make} | {wo.Vehicle.LicensePlate}",
              Labor = wo.Labor!.ToDto(),
              State = wo.State,
              IsOccupied = true,
              RepairTasks = wo.Tasks.ToDtos()
            });
          }
        }
        else
        {
          slots.Add(new AvailabilitySlotDto
          {
            IsAvailable = current >= now,
            Spot = spot,
            WorkOrderLocked = false,
            StartAtUtc = curUtc,
            EndAtUtc = nextUtc
          });
        }
        current = next;
      }
      result.Spots.Add(new SpotDto(spot,slots));
    }
    return result;
  }
}