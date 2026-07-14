using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(string Name, string Email, string PhoneNumber, List<CreateVehicleCommand> Vehicles)
  : IRequest<Result<CustomerDto>>;
