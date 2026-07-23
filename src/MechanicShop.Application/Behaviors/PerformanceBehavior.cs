using System.Diagnostics;
using MechanicShop.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly ILogger<TRequest> _logger;
  private readonly Stopwatch _stopwatch;
  private readonly IUser _user;
  private readonly IIdentityService _identityService;

  public PerformanceBehavior(ILogger<TRequest> logger,Stopwatch stopwatch, IUser user, IIdentityService identityService)
  {
    _logger = logger;
    _stopwatch = stopwatch;
    _user = user;
    _identityService = identityService;
  }
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    _stopwatch.Start();
    var response = await next(cancellationToken);
    _stopwatch.Stop();

    var elapsedMilliSeconds = _stopwatch.ElapsedMilliseconds;
    if(elapsedMilliSeconds > 500)
    {
      var userId = _user.Id ?? string.Empty;
      var requestName = typeof(TRequest).Name;
      var userName = string.Empty;
      if(userId is not null)
      {
        userName = await _identityService.GetUserNameAsync(userId, cancellationToken);
      }
      _logger.LogWarning(
          "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}", requestName, elapsedMilliSeconds, userId, userName, request);
    }
    return response;
  }
}