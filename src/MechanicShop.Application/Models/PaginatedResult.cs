namespace MechanicShop.Application.Models;

public class PaginatedResult<T>
{
  public int PageNumber { get; init; }
  public int PageSize { get; init; }
  public int TotalCount { get; init; }
  public int TotalPages => (int)Math.Ceiling((decimal)TotalCount / PageSize);
  public IReadOnlyCollection<T>? Items { get; init; }
}