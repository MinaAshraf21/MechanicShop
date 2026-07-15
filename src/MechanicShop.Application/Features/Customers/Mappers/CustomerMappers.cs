using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Application.Features.Customers.Mappers;

public static class CustomerMappers
{
  public static VehicleDto ToDto(this Vehicle vehicle)
  {
    ArgumentNullException.ThrowIfNull(vehicle);
    return new VehicleDto(vehicle.Id, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.LicensePlate);
  }

  public static List<VehicleDto> ToDtos(this IEnumerable<Vehicle> vehicles)
  {
    ArgumentNullException.ThrowIfNull(vehicles);
    // return vehicles.Select(v => v.ToDto()).ToList();
    return [.. vehicles.Select(v => v.ToDto())];
  }

  public static CustomerDto ToDto(this Customer customer)
  {
    ArgumentNullException.ThrowIfNull(customer);
    return new CustomerDto(customer.Id, customer.Name, customer.Email,customer.PhoneNumber,customer.Vehicles?.ToDtos() ?? []);
  }

    public static List<CustomerDto> ToDtos(this IEnumerable<Customer> customers)
  {
    ArgumentNullException.ThrowIfNull(customers);
    // return customers.Select(v => v.ToDto()).ToList();
    return [.. customers.Select(c => c.ToDto())];
  }
}