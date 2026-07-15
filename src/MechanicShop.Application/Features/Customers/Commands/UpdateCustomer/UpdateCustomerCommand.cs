using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(Guid CustomerId, string Name, string Email, string PhoneNumber, List<UpdateVehicleCommand> Vehicles)
  : IRequest<Result<Updated>>;