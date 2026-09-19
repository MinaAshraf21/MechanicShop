using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.AssignLabor;

public class AssignLaborCommandValidatorTests
{
  private readonly AssignLaborCommandValidator _validator = new();

  [Fact]
  public void Should_Not_Have_Error_When_Command_Is_Valid()
  {
    // Given
    var command = new AssignLaborCommand(Guid.NewGuid(), Guid.NewGuid());
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.True(result.IsValid);
  }

  [Fact]
  public void Should_Have_Error_When_WorkOrderId_Is_Empty()
  {
    // Given
    var command = new AssignLaborCommand(Guid.NewGuid(), Guid.Empty);
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.False(result.IsValid);
    Assert.Equal("WorkOrder_Id_Empty", result.Errors[0].ErrorCode);
  }

  [Fact]
  public void Should_Have_Error_When_LaborId_Is_Empty()
  {
    // Given
    var command = new AssignLaborCommand(Guid.Empty, Guid.NewGuid());
    // When
    var result = _validator.Validate(command);
    // Then
    Assert.False(result.IsValid);
    Assert.Equal("Labor_Id_Empty", result.Errors[0].ErrorCode);
  }
}