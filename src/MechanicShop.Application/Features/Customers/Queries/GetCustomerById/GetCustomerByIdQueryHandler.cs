using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(IAppDbContext context, ILogger<GetCustomerByIdQueryHandler> logger) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
  public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
  {
    var customer = await context.Customers
                                .Include(c => c.Vehicles)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
    if(customer is null)
    {
      logger.LogWarning("Customer with id {CustomerId} was not found", request.CustomerId);
      return ApplicationErrors.CustomerNotFound(request.CustomerId);
    }

    return customer.ToDto();
  }
}