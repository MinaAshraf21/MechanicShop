using Asp.Versioning;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrdersStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
[ApiVersion("1.0")]
[OutputCache(Duration = 120)]
public sealed class DashboardController(ISender sender) : ApiController
{
  [HttpGet("stats")]
  [ProducesResponseType(typeof(TodayWorkOrdersStatsDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> GetStats(DateOnly? todayDate, CancellationToken ct)
  {
    var date = todayDate ?? DateOnly.FromDateTime(DateTime.Now);
    var result = await sender.Send(new GetWorkOrdersStatsQuery(date), ct);
    return result.Match(r => Ok(r), Problem);
  }

}