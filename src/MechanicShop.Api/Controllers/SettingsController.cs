using Asp.Versioning;
using MechanicShop.Contracts.Responses;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MechanicShop.Api.Controllers;

[Route("api/settings")]
[ApiVersionNeutral] //ignores API versioning and responds to all versions the same way.
public class SettingsController(IOptions<AppSettings> options) : ApiController
{
  [HttpGet]
  [ProducesResponseType(typeof(OperatingHoursResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetOperatingHours")]
  [EndpointSummary("Gets the application's operating hours.")]
  [EndpointDescription("Returns the current configured opening and closing times.")]
  public IActionResult GetOperatingHours()
  {
    return Ok(new OperatingHoursResponse(options.Value.OpeningTime, options.Value.ClosingTime));
  }
}