using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MechanicShop.Api.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
  protected ActionResult Problem(List<Error> errors)
  {
    if(errors.Count == 0)
    {
      return Problem();
    }

    if(errors.All(e => e.Type == ErrorType.Validation))
    {
      return ValidationProblem(errors);
    }

    return Problem(errors[0]);
  }

  private ActionResult ValidationProblem(List<Error> errors)
  {
    var modelStateDictionary = new ModelStateDictionary();

    errors.ForEach(e => modelStateDictionary.AddModelError(e.Code, e.Description));

    return ValidationProblem(modelStateDictionary);
  }
  private ObjectResult Problem(Error error)
  {
    var statusCode = error.Type switch
    {
      ErrorType.Conflict => StatusCodes.Status409Conflict,
      ErrorType.Validation => StatusCodes.Status400BadRequest,
      ErrorType.NotFound => StatusCodes.Status404NotFound,
      ErrorType.Forbidden => StatusCodes.Status403Forbidden,
      ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
      _ => StatusCodes.Status500InternalServerError
    };

    return Problem(statusCode: statusCode, title: error.Description);
  }

}