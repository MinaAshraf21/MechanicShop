using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Models;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(int PageNumber=1, int PageSize=10)
  : ICachedQuery<Result<PaginatedResult<CustomerDto>>>
{
  public string CacheKey => "customers";

  public string[] Tags => ["customer"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}