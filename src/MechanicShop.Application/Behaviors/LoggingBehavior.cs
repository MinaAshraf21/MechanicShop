using MechanicShop.Application.Abstractions;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Behaviors;

public class LoggingBehavior<TRequest>(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull 
{
  public async Task Process(TRequest request, CancellationToken cancellationToken)
  {
    var requestName = typeof(TRequest).Name;
    var userId = user.Id ?? string.Empty;
    var userName = string.Empty;
    if (!string.IsNullOrEmpty(userId))
    {
      userName = await identityService.GetUserNameAsync(userId, cancellationToken);
    }
    logger.LogInformation("Request: {@Name} {@UserId} {@UserName} {@Request}", requestName, userId, userName, request);
  }
}