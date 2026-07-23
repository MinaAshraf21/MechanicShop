using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Behaviors;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(
  ILogger<TRequest> logger
)
  : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    try
    {
      return await next(cancellationToken);
    }
    catch (Exception ex)
    {
      var requestName = typeof(TRequest).Name;
      logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
      throw;
    }
  }
}