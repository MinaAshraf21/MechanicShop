using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler(
                                                IAppDbContext dbContext,
                                                ILogger<CreateCustomerCommandHandler> logger,
                                                HybridCache cache
                                                ) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
  public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
  {
    var email = request.Email.Trim().ToLower();
    var exists = await dbContext.Customers.AnyAsync(c => c.Email!.ToLower() == email);

    if (exists)
    {
      logger.LogWarning("Customer creation aborted. Email already exists.");
      return ApplicationErrors.CustomerExists;
    }

    List<Vehicle> vehicles = [];

    foreach (var v in request.Vehicles)
    {
      var vehicleResult = Vehicle.Create(Guid.NewGuid(), v.Make, v.Model, v.Year, v.LicensePlate);
      if (vehicleResult.IsFailure)
      {
        return vehicleResult.Errors!;
      }
      vehicles.Add(vehicleResult.Value);
    }

    var customerResult =  Customer.Create(Guid.NewGuid(),
                                              request.Name.Trim(),
                                              request.Email.Trim(),
                                              request.PhoneNumber.Trim(),
                                              vehicles);

    if (customerResult.IsFailure)
    {
      return customerResult.Errors!;
    }

    var customer = customerResult.Value;
    dbContext.Customers.Add(customer);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation($"Customer added with ID: {customer.Id}");
    await cache.RemoveByTagAsync("customer", cancellationToken);

    return customer.ToDto();
  }
}