using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler(
                                                IAppDbContext context,
                                                ILogger<UpdateCustomerCommandHandler> logger,
                                                HybridCache cache
      ) : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
  public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
  {
    var customer = await context.Customers
                                .Include(c => c.Vehicles)
                                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

    if(customer is null)
    {
      logger.LogWarning("Customer to update with id: {customerId} was not found", request.CustomerId);
      return ApplicationErrors.CustomerNotFound(request.CustomerId);
    }
    
    var vehicles = new List<Vehicle>();

    foreach (var v in request.Vehicles)
    {
      var vehicleResult = Vehicle.Create(v.Id ?? Guid.NewGuid(), v.Make, v.Model, v.Year, v.LicensePlate);
      if (vehicleResult.IsFailure)
      {
        return vehicleResult.Errors!;
      }
      vehicles.Add(vehicleResult.Value);
    }
    
    var customerResult = customer.Update(request.Name, request.Email, request.PhoneNumber);
    if (customerResult.IsFailure)
    {
      return customerResult.Errors!;
    }

    var updateResult = customer.UpdateParts(vehicles);
    if(updateResult.IsFailure)
      return updateResult.Errors!;

    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("customer", cancellationToken);
    return Result.Updated;

  }
}