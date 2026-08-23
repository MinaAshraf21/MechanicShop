using System.Threading.Tasks;
using Azure.Core;
using MechanicShop.Application.Behaviors;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class UnhandledExceptionBehavior
{
  private readonly ILogger<DummyRequest> _logger = Substitute.For<ILogger<DummyRequest>>();
  private readonly UnhandledExceptionBehavior<DummyRequest,Result<string>> _behavior;

  public UnhandledExceptionBehavior()
  {
    _behavior = new(_logger);
  }

  [Fact]
  public async Task Handle_WhenNoException_InvokesNextAndReturnsResult()
  {
    // Given
    var request = new DummyRequest();
    var response = (Result<string>)"test-response";
    // When
    var result = await _behavior.Handle(request, ct => Task.FromResult(response), CancellationToken.None);
    // Then
    Assert.True(result.IsSuccess);
    Assert.Equal(response.Value, result.Value);
  }

  [Fact]
  public async Task Handle_WhenExceptionOccurs_LogsErrorAndRethrowsException()
  {
    // arrange
    var request = new DummyRequest();
    var expectedException = new InvalidOperationException("test-failure");
    // act & assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>
    (
      () => _behavior.Handle(request, ct => throw expectedException, CancellationToken.None)
    );

    Assert.Equal(expectedException, exception);
    _logger.Received(1).Log
    (
      LogLevel.Error,
      Arg.Any<EventId>(),
      Arg.Is<object>(o => o.ToString()!.Contains("Unhandled Exception") &&
                      o.ToString()!.Contains(typeof(Request).Name)
                    ),
      exception,
      Arg.Any<Func<object, Exception?, string>>()
    );
  }

}