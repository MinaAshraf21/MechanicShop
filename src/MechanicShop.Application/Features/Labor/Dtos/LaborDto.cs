using MechanicShop.Domain.Identity;

namespace MechanicShop.Application.Features.Labor.Dtos;

public sealed record LaborDto(Guid LaborId, string FirstName, string LastName);