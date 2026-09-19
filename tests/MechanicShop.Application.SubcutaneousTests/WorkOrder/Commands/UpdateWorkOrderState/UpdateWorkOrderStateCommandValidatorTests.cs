using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.UpdateWorkOrderState;

public class UpdateWorkOrderStateCommandValidatorTests
{
  private readonly UpdateWorkOrderStateCommandValidator _validator = new();

  [Fact]
  public void Should_Have_Error_When_WorkOrderId_Is_Empty()
  {
    var command = new UpdateWorkOrderStateCommand(Guid.Empty, Domain.WorkOrders.Enums.State.InProgress);
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Equal("WorkOrderId_Required", result.Errors[0].ErrorCode);
  }

  [Fact]
  public void Should_Have_Error_When_Invalid_State()
  {
    var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), (Domain.WorkOrders.Enums.State)55);
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Equal("WorkOrderStatus_Invalid", result.Errors[0].ErrorCode);
  }
}