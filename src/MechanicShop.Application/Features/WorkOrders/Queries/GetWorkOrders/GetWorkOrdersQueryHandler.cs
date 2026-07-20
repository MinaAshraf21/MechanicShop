using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Models;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryHandler(
  IAppDbContext context
  ) : IRequestHandler<GetWorkOrdersQuery, Result<PaginatedResult<WorkOrderListItemDto>>>
{
  public async Task<Result<PaginatedResult<WorkOrderListItemDto>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
  {
    var workOrders = context.WorkOrders
                                    .Include(w => w.Labor)
                                    .Include(w => w.Vehicle)
                                    .Include(w => w.Tasks)
                                    .ThenInclude(t => t.Parts)
                                    .AsNoTracking().AsQueryable();

    workOrders = ApplyFilters(workOrders, request);

    if(request.SearchTerm is not null)
    {
      workOrders = ApplySearchTerm(workOrders, request.SearchTerm);
    }

    workOrders = ApplySorting(workOrders, request.SortColumn, request.SortDirection);

    var count = await workOrders.CountAsync(cancellationToken);

    var items = await workOrders
                  .Skip((request.Page -1) * request.PageSize)
                  .Take(request.PageSize)
                  .Select(wo => new WorkOrderListItemDto
                    {
                        WorkOrderId = wo.Id,
                        InvoiceId = wo.Invoice == null ? null : wo.Invoice.Id,
                        Spot = wo.Spot,
                        StartAtUtc = wo.StartAtUtc,
                        EndAtUtc = wo.EndAtUtc,
                        Vehicle = wo.Vehicle!.ToDto(),
                        Customer = wo.Vehicle!.Customer.Name,
                        Labor = wo.Labor != null
                          ? $"{wo.Labor.FirstName} {wo.Labor.LastName}"
                          : null,
                        State = wo.State,
                        RepairTasks = wo.Tasks.Select(rt => rt.Name).ToList()
                    })
                  .ToListAsync(cancellationToken);

    return new PaginatedResult<WorkOrderListItemDto>
    {
      Items = items,
      PageNumber = request.Page,
      PageSize = request.PageSize,
      TotalCount = count
    };
  }

  private IQueryable<WorkOrder> ApplyFilters(IQueryable<WorkOrder> query, GetWorkOrdersQuery searchQuery)
  {
    if (searchQuery.State.HasValue)
    {
        query = query.Where(wo => wo.State == searchQuery.State.Value);
    }

    if (searchQuery.VehicleId.HasValue && searchQuery.VehicleId != Guid.Empty)
    {
        query = query.Where(wo => wo.VehicleId == searchQuery.VehicleId.Value);
    }

    if (searchQuery.LaborId.HasValue && searchQuery.LaborId != Guid.Empty)
    {
        query = query.Where(wo => wo.LaborId == searchQuery.LaborId.Value);
    }

    if (searchQuery.StartDateFrom.HasValue)
    {
        query = query.Where(wo => wo.StartAtUtc >= searchQuery.StartDateFrom.Value);
    }

    if (searchQuery.StartDateTo.HasValue)
    {
        query = query.Where(wo => wo.StartAtUtc <= searchQuery.StartDateTo.Value);
    }

    if (searchQuery.EndDateFrom.HasValue)
    {
        query = query.Where(wo => wo.EndAtUtc >= searchQuery.EndDateFrom.Value);
    }

    if (searchQuery.EndDateTo.HasValue)
    {
        query = query.Where(wo => wo.EndAtUtc <= searchQuery.EndDateTo.Value);
    }

    if (searchQuery.Spot.HasValue)
    {
        query = query.Where(wo => wo.Spot == searchQuery.Spot.Value);
    }

    return query;
  }

  private IQueryable<WorkOrder> ApplySearchTerm(IQueryable<WorkOrder> query, string searchTerm)
  {
    var normalized = searchTerm.Trim().ToLower();

    return query.Where(wo =>
        (wo.Vehicle != null && (
            wo.Vehicle.Make.ToLower().Contains(normalized) ||
            wo.Vehicle.Model.ToLower().Contains(normalized) ||
            wo.Vehicle.LicensePlate.ToLower().Contains(normalized)
        )) ||
        (wo.Labor != null && (
            wo.Labor.FirstName.ToLower().Contains(normalized) ||
            wo.Labor.LastName.ToLower().Contains(normalized) ||
            (wo.Labor.FirstName + " " + wo.Labor.LastName).ToLower().Contains(normalized)
        )) ||
        wo.Tasks.Any(rt =>
            rt.Name.ToLower().Contains(normalized)) ||
        wo.Id.ToString().ToLower().Contains(normalized));
  }

  private IQueryable<WorkOrder> ApplySorting(IQueryable<WorkOrder> query, string sortColumn, string sortDirection)
  {
    var isDescending = sortDirection.Equals("desc", StringComparison.CurrentCultureIgnoreCase);

    return sortColumn.ToLower() switch
    {
      "createdAt" => isDescending ? query.OrderByDescending(wo => wo.CreatedAtUtc) : query.OrderBy(wo => wo.CreatedAtUtc),
      "updatedAt" => isDescending ? query.OrderByDescending(wo => wo.LastModifiedUtc) : query.OrderBy(wo => wo.LastModifiedUtc),
      "startAt" => isDescending ? query.OrderByDescending(wo => wo.StartAtUtc) : query.OrderBy(wo => wo.StartAtUtc),
      "endAt" => isDescending ? query.OrderByDescending(wo => wo.EndAtUtc) : query.OrderBy(wo => wo.EndAtUtc),
      "state" => isDescending ? query.OrderByDescending(wo => wo.State) : query.OrderBy(wo => wo.State),
      "spot" => isDescending ? query.OrderByDescending(wo => wo.Spot) : query.OrderBy(wo => wo.Spot),
      "total" => isDescending ? query.OrderByDescending(wo => wo.TotalCost) : query.OrderBy(wo => wo.TotalCost),
      "vehicleId" => isDescending ? query.OrderByDescending(wo => wo.VehicleId) : query.OrderBy(wo => wo.VehicleId),
      "laborId" => isDescending ? query.OrderByDescending(wo => wo.LaborId) : query.OrderBy(wo => wo.LaborId),
      _ => query.OrderByDescending(wo => wo.CreatedAtUtc) // Default sorting
    };

    
  }
}