using MediatR;

namespace MechanicShop.Application.Abstractions;

public interface ICachedQuery
{
  string CacheKey { get; }
  string[] Tags { get; }
  TimeSpan Expiration { get; }
}

public interface ICachedQuery<TRequest> : ICachedQuery, IRequest<TRequest>;
