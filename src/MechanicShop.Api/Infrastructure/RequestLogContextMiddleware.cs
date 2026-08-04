using Serilog.Context;

namespace MechanicShop.Api.Infrastructure;

public class RequestLogContextMiddleware(RequestDelegate next)
{
  public Task InvokeAsync(HttpContext httpContext)
  {
    using(LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
    {
      return next(httpContext);
    }
  }
}