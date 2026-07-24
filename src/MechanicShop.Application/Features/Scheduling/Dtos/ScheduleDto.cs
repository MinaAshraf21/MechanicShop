namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed record ScheduleDto(DateOnly OnDate, bool EndOfDay, List<SpotDto> Spots);