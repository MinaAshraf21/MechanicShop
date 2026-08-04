
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using MechanicShop.Application.Features.Labor.Queries.GetLabors;
using Microsoft.AspNetCore.OutputCaching;
using MechanicShop.Application.Features.Labor.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace MechanicShop.Api.Controllers;

[Route("/api/v{version:apiVersion}/employees")]
[ApiVersion("1.0")]
public class EmployeeController(ISender sender) : ApiController
{
  [HttpGet]
  [Authorize(Roles = "Manager")]
  [EndpointName("GetLabors")]
  [EndpointSummary("Gets all labors.")]
  [EndpointDescription("Gets all labors registered in the system.")]
  [OutputCache(Duration = 60)]
  [ProducesResponseType(typeof(List<LaborDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> GetAll(CancellationToken ct)
  {
    var result = await sender.Send(new GetLaborsQuery(), ct);
    return result.Match(Ok, Problem);
  }
}