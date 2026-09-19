using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandValidatorTests
{
  private readonly CreateWorkOrderCommandValidator _validator;

  public CreateWorkOrderCommandValidatorTests()
  {
    _validator = new();
  }

  [Fact]
  public void WhenVehicleIdIsEmpty_ThenReturnsFalse()
  {
    // Given
    var command = new CreateWorkOrderCommand(
          Guid.Empty,
          DateTimeOffset.UtcNow.AddHours(1),
          Guid.NewGuid(),
          Domain.WorkOrders.Enums.Spot.A,
          [Guid.NewGuid()]
        );
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "VehicleId");
  }

  [Fact]
  public void WhenStartAtNotInTheFuture_ThenReturnsFalse()
  {
    // Given
    var command = new CreateWorkOrderCommand(
          Guid.NewGuid(),
          DateTimeOffset.UtcNow.AddHours(-1),
          Guid.NewGuid(),
          Domain.WorkOrders.Enums.Spot.A,
          [Guid.NewGuid()]
        );
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "startAt");
  }

  [Fact]
  public void WhenLaborIdIsEmpty_ThenReturnsFalse()
  {
    // Given
    var command = new CreateWorkOrderCommand(
          Guid.NewGuid(),
          DateTimeOffset.UtcNow.AddHours(1),
          Guid.Empty,
          Domain.WorkOrders.Enums.Spot.A,
          [Guid.NewGuid()]
        );
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "laborId");
  }

  [Fact]
  public void WhenRepairTasksIdsAreEmpty_ThenReturnsFalse()
  {
    // Given
    var command = new CreateWorkOrderCommand(
          Guid.NewGuid(),
          DateTimeOffset.UtcNow.AddHours(1),
          Guid.NewGuid(),
          Domain.WorkOrders.Enums.Spot.A,
          []
        );
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "RepairTaskIds");
  }

  [Fact]
  public void WhenSpotInvalid_ThenReturnsFalse()
  {
    // Given
    var command = new CreateWorkOrderCommand(
          Guid.NewGuid(),
          DateTimeOffset.UtcNow.AddHours(1),
          Guid.NewGuid(),
          (Domain.WorkOrders.Enums.Spot)55,
          [Guid.NewGuid()]
        );
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "Spot");
  }

  [Fact]
  public void WhenValidData_ThenReturnsTrue()
  {
    // Given
    var command = new CreateWorkOrderCommand(
          Guid.NewGuid(),
          DateTimeOffset.UtcNow.AddHours(1),
          Guid.NewGuid(),
          Domain.WorkOrders.Enums.Spot.A,
          [Guid.NewGuid()]
        );
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.True(result.IsValid);
  }

}