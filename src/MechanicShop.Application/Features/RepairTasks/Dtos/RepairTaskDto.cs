using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.Features.RepairTasks.Dtos;

public sealed record RepairTaskDto(Guid Id, string Name, decimal LaborCost, RepairDurationInMinutes Duration, List<PartDto> Parts);