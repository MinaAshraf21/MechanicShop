using System.Threading.Tasks;
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Behaviors;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class PerformanceBehaviorTests
{
  private readonly IUser _user = Substitute.For<IUser>();
  private readonly ILogger<DummyRequest> _logger = Substitute.For<ILogger<DummyRequest>>();
  private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
  private readonly PerformanceBehavior<DummyRequest, Result<string>> _sut;

  public PerformanceBehaviorTests()
  {
    _sut = new(_logger, _user, _identityService);
  }

  [Fact]
  public async Task Handle_WhenRequestTakesMoreThan500Ms_ShouldLogWarning()
  {
    // Given
    var request = new DummyRequest();
    var expectedResponse = "Success";
    var userId = "abc123";
    var userName = "Mina";
    _user.Id.Returns(userId);
    _identityService.GetUserNameAsync(userId).Returns(userName);
    // When
    var response = await _sut.Handle(request, async _ =>
    {
      await Task.Delay(600, CancellationToken.None);
      return expectedResponse;
    }, CancellationToken.None);

    // Then
    Assert.Equal(expectedResponse, response.Value);
    _logger.Received(1)
            .Log
            (
              LogLevel.Warning,
              Arg.Any<EventId>(),
              Arg.Is<object>
                (
                  o =>
                    o.ToString()!.Contains("Long Running Request") &&
                    o.ToString()!.Contains("DummyRequest") &&
                    o.ToString()!.Contains(userId) &&
                    o.ToString()!.Contains(userName)
                ),
              null,
              Arg.Any<Func<object, Exception?, string>>()
            );
  }

  [Fact]
  public async Task Handle_WhenRequestTakesLessThan500Ms_ShouldNotLogWarning()
  {
    // Given
    var request = new DummyRequest();
    var expectedResponse = "Success";
    // When
    var response = await _sut.Handle(request, _ => Task.FromResult((Result<string>)expectedResponse), CancellationToken.None);
    // Then
    Assert.Equal(expectedResponse, response.Value);
    _logger.DidNotReceive().Log(
              LogLevel.Warning,
              Arg.Any<EventId>(),
              Arg.Any<object>(),
              Arg.Any<Exception>(),
              Arg.Any<Func<object, Exception?, string>>()
            );

  }

  [Fact]
  public async Task Handle_WhenNextThrowsException_ShouldNotCatchException()
  {
    // Arrange
    var request = new DummyRequest();
    var expectedException = new InvalidOperationException("Test exception");

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => _sut.Handle(request, (_) => throw expectedException, CancellationToken.None));

    Assert.Equal(expectedException, exception);
  }
}