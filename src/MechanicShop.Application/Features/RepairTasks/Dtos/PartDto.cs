namespace MechanicShop.Application.Features.RepairTasks.Dtos;

public sealed record PartDto(Guid Id, string Name, decimal Cost, int Quantity);