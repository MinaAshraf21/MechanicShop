
namespace MechanicShop.Application.Features.Customers.Dtos;

public record CustomerDto(string Name, string? Email, string PhoneNumber, List<VehicleDto> Vehicles);
