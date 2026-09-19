using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
  private readonly UpdateWorkOrderRepairTasksCommandValidator _validator = new();

  [Fact]
  public void Should_Have_Error_When_WorkOrderId_Is_Empty()
  {
    var command = new UpdateWorkOrderRepairTasksCommand(Guid.Empty, [Guid.NewGuid()]);
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Equal("WorkOrderId_Required", result.Errors[0].ErrorCode);
  }

  [Fact]
  public void Should_Have_Error_When_RepairTasks_Is_Empty()
  {
    var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), []);
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Equal("RepairTasks_Required", result.Errors[0].ErrorCode);
  }
}