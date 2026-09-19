using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.DeleteWorkOrder;

public class DeleteWorkOrderCommandValidatorTests
{
private readonly DeleteWorkOrderCommandValidator _sut = new();

  [Fact]
  public void Should_Have_Error_When_WorkOrderId_Is_Empty()
  {
    var command = new DeleteWorkOrderCommand(Guid.Empty);
    var result = _sut.Validate(command);

    Assert.False(result.IsValid);
    Assert.Equal("WorkOrderId_Required", result.Errors[0].ErrorCode);
  }

  [Fact]
  public void Should_Not_Have_Error_When_WorkOrderId_Is_Valid()
  {
    var command = new DeleteWorkOrderCommand(Guid.NewGuid());
    var result = _sut.Validate(command);

    Assert.True(result.IsValid);
  }
}