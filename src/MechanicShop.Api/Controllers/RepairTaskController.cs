using Asp.Versioning;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.DeleteRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Contracts.Requests.RepairTasks;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("/api/v{version:apiVersion}/employees")]
[ApiVersion("1.0")]
public class RepairTaskController(ISender sender) : ApiController
{
  [HttpGet]
  [Authorize]
  [EndpointName("GetRepairTasks")]
  [EndpointSummary("Gets all repair tasks.")]
  [EndpointDescription("Gets all available repair tasks in the system.")]
  [OutputCache(Duration = 60)]
  [ProducesResponseType(typeof(List<RepairTaskDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> GetAll(CancellationToken ct)
  {
    var result = await sender.Send(new GetRepairTasksQuery(), ct);
    return result.Match(Ok, Problem);
  }

  [HttpGet("{taskId:guid}")]
  [Authorize]
  [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointName("GetRepairTaskById")]
  [EndpointSummary("Retrieves a repair task by its id.")]
  [EndpointDescription("Returns detailed information about the specified repair task if found and its parts.")]
  [OutputCache(Duration = 60)]
  public async Task<ActionResult> GetById(Guid taskId, CancellationToken ct)
  {
    var result = await sender.Send(new GetRepairTaskByIdQuery(taskId), ct);
    return result.Match(Ok, Problem);
  }

  [HttpPost]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("CreateRepairTask")]
  [EndpointSummary("Creates a new repair task.")]
  [EndpointDescription("Adds a new repair task to the system.")]
  public async Task<ActionResult> CreateRepairTask(CreateRepairTaskRequest request, CancellationToken ct)
  {
    var parts = request.Parts.ConvertAll(p => new CreatePartCommand(p.Name!, p.Cost, p.Quantity));
    var command = new CreateRepairTaskCommand(
      request.Name,
      request.LaborCost,
      (RepairDurationInMinutes)request.EstimatedDuration,
      parts
      );

    var result = await sender.Send(command, ct);
    return result.Match(
      response => CreatedAtAction(nameof(GetById), new {RepairTaskId = response.Id}, response),
      Problem
    );
  }

  [HttpPut("{taskId:guid}")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("UpdateRepairTask")]
  [EndpointSummary("Updates a repair task.")]
  [EndpointDescription("Updates an existing repair task in the system.")]
  public async Task<ActionResult> UpdateRepairTask(Guid taskId, [FromBody]UpdateRepairTaskRequest request, CancellationToken ct)
  {
    var parts = request.Parts.ConvertAll(p => new UpdatePartCommand(p.PartId, p.Name!, p.Cost, p.Quantity));
    var command = new UpdateRepairTaskCommand(
      taskId,
      request.Name,
      request.LaborCost,
      (RepairDurationInMinutes)request.EstimatedDuration,
      parts
      );

    var result = await sender.Send(command, ct);
    return result.Match(
      response => NoContent(),
      Problem
    );
  }

    [HttpDelete("{taskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("RemoveRepairTask")]
    [EndpointSummary("Removes a repair task.")]
    [EndpointDescription("Deletes the specified repair task from the system.")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid taskId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteRepairTaskCommand(taskId), ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }
}