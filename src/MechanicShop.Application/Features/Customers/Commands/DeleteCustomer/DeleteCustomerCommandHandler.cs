using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandHandler(
    ILogger<DeleteCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<DeleteCustomerCommand, Result<Deleted>>
{
  public async Task<Result<Deleted>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
  {
    var customer = await context.Customers
                          .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
    
    if(customer is null)
    {
      logger.LogWarning("Customer with id: {customerId} was not found for deletion", request.CustomerId);
      return ApplicationErrors.CustomerNotFound(request.CustomerId);
    }

    bool hasCurrentWorkOrders = await context.WorkOrders
                                .Include(w => w.Vehicle)
                                .Where(w => w.Vehicle != null)
                                .AnyAsync(w => w.Vehicle!.CustomerId == customer.Id, cancellationToken);

    if (hasCurrentWorkOrders)
    {
      logger.LogWarning("Customer {CustomerId} cannot be deleted because they have associated work orders (past, scheduled, or in-progress).", request.CustomerId);
      return CustomerErrors.CannotDeleteCustomerWithWorkOrders;
    }

    context.Customers.Remove(customer);
    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("customer", cancellationToken);
    logger.LogInformation("Customer with Id: {CustomerId} is deleted successfully.", request.CustomerId);

    return Result.Deleted;
  }
}