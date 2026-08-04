using Asp.Versioning;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.Scheduling.GetDailySchedule;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Contracts.Common;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("/api/v{version:apiVersion}/work-orders")]
[ApiVersion("1.0")]
public class WorkOrderController(ISender sender) : ApiController
{
  [HttpGet]
  [Authorize]
  [ProducesResponseType(typeof(List<WorkOrderDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetPagedWorkOrders")]
  [EndpointSummary("Retrieves a paginated list of work orders.")]
  [EndpointDescription("Supports filtering by date range, status, vehicle, labor, spot, and searching by term. Pagination and sorting are supported.")]
  [OutputCache(Duration = 60)]
  public async Task<ActionResult> GetPagedWorkOrders(PageRequest request, CancellationToken ct)
  {
    if(request.Page <= 0)
      request.Page = 1;
    if(request.PageSize <= 0)
      request.Page = 1;

    var query = new GetWorkOrdersQuery(
      request.Page,
      request.PageSize,
      request.SearchTerm,
      request.SortColumn,
      request.SortDirection,
      request.State is null ? null : (State)(int)request.State,
      request.VehicleId,
      request.LaborId,
      request.StartDateFrom,
      request.StartDateTo,
      request.EndDateFrom,
      request.EndDateTo,
      request.Spot is null ? null : (Domain.WorkOrders.Enums.Spot)(int)request.Spot
    );
    var result = await sender.Send(query, ct);
    return result.Match(Ok, Problem);
  }

  [HttpGet("{workOrderId:guid}")]
  [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetWorkOrderById")]
  [EndpointSummary("Retrieves a work order by its ID.")]
  [EndpointDescription("Returns detailed information about the specified work order if it exists.")]
  public async Task<ActionResult> GetWorkOrderById(Guid workOrderId,CancellationToken ct)
  {
    var result = await sender.Send(new GetWorkOrderByIdQuery(workOrderId), ct);
    return result.Match(Ok, Problem);
  }

  [HttpPost]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("CreateWorkOrder")]
  [EndpointSummary("Creates a new work order.")]
  [EndpointDescription("Creates a new work order for a vehicle, specifying labor, tasks, and other required information.")]
  public async Task<ActionResult> CreateWorkOrder(CreateWorkOrderRequest request, CancellationToken ct)
  {
    var command = new CreateWorkOrderCommand(
      request.VehicleId,
      request.StartAt,
      request.LaborId,
      (Domain.WorkOrders.Enums.Spot)(int)request.Spot,
      request.RepairTasksIds
    );
    var result = await sender.Send(command, ct);
    return result.Match(Ok, Problem);
  }

  [HttpPut("{workOrderId:guid}/relocation")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("RelocateWorkOrder")]
  [EndpointSummary("Relocates a work order to a new time and spot.")]
  [EndpointDescription("Updates the scheduled time and assigned bay for a work order. Only users with the Manager role can perform this action.")]
  public async Task<ActionResult> RelocateWorkOrder(Guid workOrderId, [FromBody]RelocateWorkOrderRequest request, CancellationToken ct)
  {
    var result = await sender.Send(new RelocateWorkOrderCommand(workOrderId, request.NewStartAtUtc, (Domain.WorkOrders.Enums.Spot)(int)request.NewSpot), ct);
    return result.Match(_ => NoContent(), Problem);
  }

  [HttpPut("{workOrderId:guid}/labor")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("RelocateWorkOrder")]
  [EndpointSummary("Assigns a labor to a work order.")]
  [EndpointDescription("Associates a labor definition with a specific work order. Only managers can perform this operation.")]
  public async Task<ActionResult> AssignLabor(Guid workOrderId, [FromBody]AssignLaborRequest request, CancellationToken ct)
  {
    var result = await sender.Send(new AssignLaborCommand(request.LaborId, workOrderId), ct);
    return result.Match(_ => NoContent(), Problem);
  }

  [HttpPut("{workOrderId:guid}/state")]
  [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Labor)}", Policy = "SelfScopedWorkOrderAccess")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointSummary("Changes the state of a work order.")]
  [EndpointDescription("Updates the current state of the specified work order. Only users with the Manager role are authorized.")]
  [EndpointName("UpdateWorkOrderState")]
  [MapToApiVersion("1.0")]
  public async Task<IActionResult> UpdateState(Guid workOrderId, UpdateWorkOrderStateRequest request, CancellationToken ct)
  {
      var command = new UpdateWorkOrderStateCommand(
          workOrderId,
          (State)(int)request.State);

      var result = await sender.Send(command, ct);

      return result.Match(
          _ => NoContent(),
          Problem);
  }

  [HttpPut("{workOrderId:guid}/repair-task")]
  [Authorize(Roles = nameof(Role.Manager))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UpdateRepairTasks(Guid workOrderId, UpdateRepairTasksRequest request, CancellationToken ct)
  {
      var command = new UpdateWorkOrderRepairTasksCommand(workOrderId, request.RepairTasksIds);

      var result = await sender.Send(command, ct);

      return result.Match(
          _ => NoContent(),
          Problem);
  }

  [HttpDelete("{workOrderId:guid}")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("DeleteWorkOrder")]
  [EndpointSummary("Deletes a work order.")]
  [EndpointDescription("Deletes the specified work order permanently. Only users with the Manager role are authorized.")]
  public async Task<IActionResult> Delete(Guid workOrderId, CancellationToken ct)
  {
      var result = await sender.Send(new DeleteWorkOrderCommand(workOrderId), ct);

      return result.Match(
            _ => NoContent(),
            Problem);
  }

  [HttpGet("schedule/{date}")]
  [Authorize]
  [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointSummary("Retrieves the schedule for a given day.")]
  [EndpointDescription("Returns a schedule view for the specified date. If no date is provided, today's schedule is returned. You can optionally filter by labor ID.")]
  [EndpointName("GetDailySchedule")]
  public async Task<IActionResult> GetSchedule(
    DateOnly? date,
    [FromQuery] Guid? laborId,
    [FromHeader(Name = "X-TimeZone")] string? tz,
    CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(tz))
    {
      return Problem(
          detail: "Missing time zone in 'X-TimeZone' header.",
          statusCode: StatusCodes.Status400BadRequest,
          title: "Time Zone Required");
    }

    TimeZoneInfo timeZone;

    try
    {
      timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
    }
    catch
    {
      return Problem(
          detail: $"Invalid or unknown time zone: '{tz}'.",
          statusCode: StatusCodes.Status400BadRequest,
          title: "Invalid Time Zone");
    }

    var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

    var result = await sender.Send(new GetDailyScheduleQuery(scheduleDate, laborId, timeZone), ct);

    return result.Match(
        response => Ok(response),
        Problem);
  }

}