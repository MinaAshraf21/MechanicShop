using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Abstractions;

public interface IWorkOrderPolicy
{
  Task<bool> IsLaborOccupied(Guid LaborId, Guid excludedWorkOrderId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc);
  bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration);
  Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null);
  Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default);
  Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt);
}