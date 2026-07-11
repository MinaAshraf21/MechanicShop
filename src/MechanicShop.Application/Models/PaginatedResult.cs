namespace MechanicShop.Application.Models;

public class PaginatedResult<T>
{
  public int PageNumber { get; init; }
  public int PageSize { get; init; }
  public int TotalCount { get; init; }
  public int TotalPages { get; init; }
  public IReadOnlyCollection<T>? Items { get; init; }
}