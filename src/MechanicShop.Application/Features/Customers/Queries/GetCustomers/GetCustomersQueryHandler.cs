using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Models;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler(IAppDbContext context, ILogger<GetCustomersQueryHandler> logger)
:  IRequestHandler<GetCustomersQuery, Result<PaginatedResult<CustomerDto>>>
{
  public async Task<Result<PaginatedResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
  {
    var customers = await context.Customers
                                  .Include(c => c.Vehicles)
                                  .AsNoTracking()
                                  .Skip((request.PageNumber - 1) * request.PageSize)
                                  .Take(request.PageSize)
                                  .ToListAsync(cancellationToken);

    var customersDto = customers.ToDtos();

    var paginatedResult = new PaginatedResult<CustomerDto>
    {
      PageNumber = request.PageNumber,
      PageSize = request.PageSize,
      Items = customersDto
    };

    return paginatedResult;
  }
}