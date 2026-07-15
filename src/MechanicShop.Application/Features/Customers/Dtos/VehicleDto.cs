namespace MechanicShop.Application.Features.Customers.Dtos;

public record VehicleDto(Guid Id, string Make,string Model, int Year, string LicensePlate);