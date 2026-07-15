using MechanicShop.Application.Abstractions;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
      HybridCache cache,
      ILogger<CachingBehavior<TRequest, TResponse>> logger
      ) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    if(request is not ICachedQuery cachedRequest)
      return await next(cancellationToken);

    logger.LogInformation("Checking cache for request name: {cachedRequestName}", typeof(TRequest).Name);

    var result = await cache.GetOrCreateAsync(
      cachedRequest.CacheKey,
      async ct => {
        var result = await next(ct); // in case data is not cached => execute the request handler to get the data from the DB and then cache it
        if(result is IResult r && r.IsSuccess)
        {
          return result;
        }
        // means (result.IsFailure = true) so we will not cache failures
        return default!;
      },
      new HybridCacheEntryOptions {
        Expiration = cachedRequest.Expiration
      },
      cachedRequest.Tags,
      cancellationToken
    );

    return result;
  }
}