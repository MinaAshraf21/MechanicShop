using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.Services;

public sealed class WorkOrderPolicy(
  IAppDbContext context,
  IOptions<AppSettings> options
) : IWorkOrderPolicy
{
  public async Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default)
  {
    var isOccupied = await context.WorkOrders
                              .AnyAsync
                              (
                                w => w.Spot == spot &&
                                w.StartAtUtc < endAt &&
                                w.EndAtUtc > startAt &&
                                (excludeWorkOrderId != null ? w.Id != excludeWorkOrderId : true),
                                ct);
    if(isOccupied)
      return Result.Success;
    return Error.Conflict("MechanicShop_SpotTimeSlot_Unavailable", "The selected time slot is unavailable for the requested services.");
  }

  public async Task<bool> IsLaborOccupied(Guid LaborId, Guid excludedWorkOrderId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken ct = default)
  {
    return await context.WorkOrders
                              .AnyAsync
                              ( w =>
                                w.LaborId == LaborId &&
                                w.StartAtUtc < endAtUtc &&
                                w.EndAtUtc > startAtUtc &&
                                (w.Id != excludedWorkOrderId ),ct);
  }

  public bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration)
  {
    // var closingTime = options.Value.ClosingTime;
    // var openingTime = options.Value.OpeningTime;
    var opening = startAt.Date.Add(options.Value.OpeningTime.ToTimeSpan());
    var closing = startAt.Date.Add(options.Value.ClosingTime.ToTimeSpan());
    var endAt = startAt + duration;

    // return TimeOnly.FromDateTime(startAt.DateTime) < openingTime || TimeOnly.FromDateTime(endAt.DateTime) > closingTime;
    return startAt < opening || endAt > closing;
  }

  public async Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null)
  {
    return await context.WorkOrders
                              .AnyAsync
                              ( w =>
                                w.VehicleId == vehicleId &&
                                w.StartAtUtc < endAt &&
                                w.EndAtUtc > startAt &&
                                (excludedWorkOrderId != null ? w.Id != excludedWorkOrderId : true));
  }

  public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
  {
    var min = options.Value.MinimumAppointmentDurationInMinutes;
    if(endAt.Subtract(startAt).TotalMinutes < min)
        return Error.Conflict(
          "WorkOrder_TooShort",
          $"WorkOrder duration must be at least {options.Value.MinimumAppointmentDurationInMinutes} minutes.");

    return Result.Success;
  }
}