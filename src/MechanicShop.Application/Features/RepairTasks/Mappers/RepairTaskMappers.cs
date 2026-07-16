using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class RepairTaskMappers
{
  public static RepairTaskDto ToDto(this RepairTask repairTask)
  {
    return new RepairTaskDto(repairTask.Id, repairTask.Name, repairTask.LaborCost, repairTask.EstimatedDuration, repairTask.Parts.ToDtos());
  }
  public static List<RepairTaskDto> ToDtos(this IEnumerable<RepairTask> repairTasks)
  {
    return [.. repairTasks.Select(r => r.ToDto())];
  }
  public static PartDto ToDto(this Part part)
  {
    return new PartDto(part.Id,part.Name!,  part.Cost, part.Quantity);
  }
  public static List<PartDto> ToDtos(this IEnumerable<Part> parts)
  {
    return [.. parts.Select(p => p.ToDto())];
  }
}