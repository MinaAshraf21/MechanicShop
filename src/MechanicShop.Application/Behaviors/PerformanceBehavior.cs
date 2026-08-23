using System.Diagnostics;
using MechanicShop.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly ILogger<TRequest> _logger;
  private readonly IUser _user;
  private readonly IIdentityService _identityService;

  public PerformanceBehavior(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
  {
    _logger = logger;
    _user = user;
    _identityService = identityService;
  }
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var stopwatch = Stopwatch.StartNew();
    var response = await next(cancellationToken);
    stopwatch.Stop();

    var elapsedMilliSeconds = stopwatch.ElapsedMilliseconds;
    if(elapsedMilliSeconds > 500)
    {
      var userId = _user.Id ?? string.Empty;
      var requestName = typeof(TRequest).Name;
      var userName = string.Empty;
      if(userId is not null)
      {
        userName = await _identityService.GetUserNameAsync(userId);
      }
      _logger.LogWarning(
          "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}", requestName, elapsedMilliSeconds, userId, userName, request);
    }
    return response;
  }
}