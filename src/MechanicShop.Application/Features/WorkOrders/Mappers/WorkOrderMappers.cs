using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Mappers;



public static class WorkOrderMapper
{
    public static WorkOrderDto ToDto(this WorkOrder entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new WorkOrderDto
        {
            WorkOrderId = entity.Id,
            Spot = entity.Spot,
            StartAt = entity.StartAtUtc,
            EndAt = entity.EndAtUtc,
            Labor = entity.Labor is null ? null : new LaborDto(entity.LaborId, entity.Labor.FirstName, entity.Labor.LastName),
            RepairTasks = entity.Tasks.ToDtos(),
            Vehicle = entity.Vehicle is null ? null : entity.Vehicle.ToDto(),
            State = entity.State,
            TotalPartCost = entity.Tasks.SelectMany(t => t.Parts).Sum(p => p.Cost * p.Quantity),
            TotalLaborCost = entity.Tasks.Sum(p => p.LaborCost),
            TotalCost = entity.Tasks.Sum(rt => rt.TotalCost),
            TotalDurationInMins = entity.Tasks.Sum(rt => (int)rt.EstimatedDuration),
            InvoiceId = entity.Invoice!.Id,
            CreatedAt = entity.CreatedAtUtc
        };
    }

    public static List<WorkOrderDto> ToDtos(this IEnumerable<WorkOrder> entities)
    {
        return [.. entities.Select(e => e.ToDto())];
    }

    // public static WorkOrderListItemDto ToListItemDto(this WorkOrder entity)
    // {
    //     ArgumentNullException.ThrowIfNull(entity);

    //     return new WorkOrderListItemDto
    //     {
    //         WorkOrderId = entity.Id,
    //         Spot = entity.Spot,
    //         StartAtUtc = entity.StartAtUtc,
    //         EndAtUtc = entity.EndAtUtc,
    //         Vehicle = entity.Vehicle!.ToDto(),
    //         Labor = entity.Labor is null ? null :
    //             $"{entity.Labor.FirstName} {entity.Labor.LastName}",
    //         State = entity.State,
    //         RepairTasks = entity.RepairTasks.Select(rt => rt.Name).ToList()
    //     };
    // }
}
