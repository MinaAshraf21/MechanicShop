using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed record SpotDto(Spot Spot, List<AvailabilitySlotDto> Slots);