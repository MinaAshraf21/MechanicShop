
namespace MechanicShop.Application.Features.Customers.Dtos;

public record CustomerDto(Guid Id, string Name, string? Email, string PhoneNumber, List<VehicleDto> Vehicles);
