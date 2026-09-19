using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandValidatorTests
{
  private readonly RelocateWorkOrderCommandValidator _validator = new();

  [Fact]
  public void Should_Have_Error_When_WorkOrderId_Is_Empty()
  {
    var command = new RelocateWorkOrderCommand(Guid.Empty, DateTimeOffset.Now, Domain.WorkOrders.Enums.Spot.A);
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Equal("WorkOrderId_Required", result.Errors[0].ErrorCode);
  }

  [Fact]
  public void Should_Have_Error_When_Invalid_Spot_Value()
  {
    var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTimeOffset.Now, (Domain.WorkOrders.Enums.Spot)55);
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
  }

  [Fact]
  public void Should_Have_Error_When_StartAt_NotInFuture()
  {
    var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTimeOffset.Now.AddMinutes(-20), Domain.WorkOrders.Enums.Spot.A);
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
  }
}