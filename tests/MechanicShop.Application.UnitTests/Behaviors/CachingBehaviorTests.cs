using System.Threading.Tasks;
using Castle.Core.Logging;
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Behaviors;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class CachedQuery : ICachedQuery
{
  public string CacheKey => "test-key";

  public string[] Tags => ["unit-test"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
public class NonCachedQuery;

public class CachingBehaviorTests
{
  private readonly HybridCache _cache = Substitute.For<HybridCache>();
  private readonly ILogger<CachingBehavior<CachedQuery, Result<string>>> _logger
                  = Substitute.For<ILogger<CachingBehavior<CachedQuery, Result<string>>>>();
  private readonly CachingBehavior<CachedQuery, Result<string>> _sut;

  public CachingBehaviorTests()
  {
    _sut = new(_cache, _logger);
  }

  [Fact]
  public async Task Handle_WhenNotCachedQuery_ShouldSkipCacheAndReturnResult()
  {
    //arrange
      var uncachedRequest = new NonCachedQuery();
      var behavior = new CachingBehavior<NonCachedQuery, Result<string>>(_cache, Substitute.For<ILogger<CachingBehavior<NonCachedQuery, Result<string>>>>());
    //act
    var result = await behavior.Handle(uncachedRequest, ct => Task.FromResult<Result<string>>("Ok"), CancellationToken.None);
    //assert
    Assert.Equal("Ok", result.Value);
    await _cache.DidNotReceive()
                .GetOrCreateAsync
                  (
                    Arg.Any<string>(),
                    Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
                    Arg.Any<HybridCacheEntryOptions>(),
                    Arg.Any<string[]>(),
                    Arg.Any<CancellationToken>()
                  );
  }

  [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsSuccess_ShouldCacheResult()
    {
        // Arrange
        var request = new CachedQuery();
        var response = (Result<string>)"test-value";

        string? actualKey = null;
        HybridCacheEntryOptions? actualOptions = null;
        string[]? actualTags = null;
        bool wasNextCalled = false;

      _cache.GetOrCreateAsync(
          Arg.Do<string>(k => actualKey = k),
          Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
          Arg.Do<HybridCacheEntryOptions>(o => actualOptions = o),
          Arg.Do<string[]>(t => actualTags = t)
          ).Returns(callInfo => {
              // Get the factory function from the call
              var factory = callInfo.Arg<Func<CancellationToken, ValueTask<Result<string>>>>();
              // get data from database
              var result = factory(CancellationToken.None);
              wasNextCalled = true;
              return result;
          });

        // Act
        var result = await _sut.Handle(request, _ => Task.FromResult(response), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-value", result.Value);
        Assert.Equal(request.CacheKey, actualKey);
        Assert.True(wasNextCalled); // Verify the factory was executed
        Assert.Equal(request.Expiration, actualOptions!.Expiration);
        Assert.Equal(request.Tags, actualTags);
    }

}