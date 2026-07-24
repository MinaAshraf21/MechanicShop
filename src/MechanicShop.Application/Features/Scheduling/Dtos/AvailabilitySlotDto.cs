using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed class AvailabilitySlotDto
{
  public Guid? WorkOrderId { get; set; }
  public string? Vehicle { get; set; }
  public LaborDto? Labor { get; set; }
  public bool IsAvailable { get; set; }
  public bool IsOccupied { get; set; }
  public DateTimeOffset StartAtUtc { get; set; }
  public DateTimeOffset EndAtUtc { get; set; }
  public Spot Spot { get; set; }
  public bool WorkOrderLocked { get; set; }
  public State? State { get; set; }
  public List<RepairTaskDto>? RepairTasks { get; set; }
}