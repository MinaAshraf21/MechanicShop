using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
  [Fact]
  public void Create_ShouldReturnError_WhenIdIsEmpty()
  {
    var wo = WorkOrderFactory.CreateWorkOrder(id: Guid.Empty);

    Assert.False(wo.IsSuccess);

    Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, wo.TopError.Code);
    Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Description, wo.TopError.Description);
  }

  [Fact]
  public void Create_ShouldReturnError_WhenVehicleIdIsEmpty()
  {
    var wo = WorkOrderFactory.CreateWorkOrder(vehicleId: Guid.Empty);

    Assert.False(wo.IsSuccess);

    Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, wo.TopError.Code);
    Assert.Equal(WorkOrderErrors.VehicleIdRequired.Description, wo.TopError.Description);
  }

  [Fact]
  public void Create_ShouldReturnError_WhenLaborIdIsEmpty()
  {
    var wo = WorkOrderFactory.CreateWorkOrder(laborId: Guid.Empty);

    Assert.False(wo.IsSuccess);

    Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, wo.TopError.Code);
    Assert.Equal(WorkOrderErrors.LaborIdRequired.Description, wo.TopError.Description);
  }

  [Fact]
  public void Create_ShouldReturnError_WhenNoRepairTasks()
  {
    var wo = WorkOrderFactory.CreateWorkOrder(repairTasks: []);

    Assert.False(wo.IsSuccess);

    Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, wo.TopError.Code);
    Assert.Equal(WorkOrderErrors.RepairTasksRequired.Description, wo.TopError.Description);
  }

  [Fact]
  public void Create_ShouldReturnError_WhenTimingInvalid()
  {
    var wo = WorkOrderFactory.CreateWorkOrder(
      startAt: DateTimeOffset.UtcNow.AddHours(1),
      endAt: DateTimeOffset.UtcNow
    );

    Assert.False(wo.IsSuccess);

    Assert.Equal(WorkOrderErrors.InvalidTiming.Code, wo.TopError.Code);
    Assert.Equal(WorkOrderErrors.InvalidTiming.Description, wo.TopError.Description);
  }

  [Fact]
  public void Create_ShouldReturnError_WhenSpotInvalid()
  {
    const Spot invalidSpot = (Spot)999;

    var wo = WorkOrderFactory.CreateWorkOrder(spot: invalidSpot);

    Assert.False(wo.IsSuccess);

    Assert.Equal(WorkOrderErrors.SpotInvalid.Code, wo.TopError.Code);
  }

  [Fact]
  public void AddRepairTask_ShouldReturnError_WhenNotEditable()
  {
    var wo = WorkOrder.Create(
                id: Guid.NewGuid(),
                vehicleId: Guid.NewGuid(),
                startAt: DateTimeOffset.UtcNow,
                endAt: DateTimeOffset.UtcNow.AddHours(1),
                laborId: Guid.NewGuid(),
                spot: Spot.A,
                repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

    wo.UpdateState(State.InProgress);
    var result = wo.AddRepairTask(RepairTasksFactory.CreateRepairTask().Value);

    Assert.True(result.IsFailure);
  }

  [Fact]
  public void UpdateLabor_ShouldReturnError_WhenLaborIdEmpty()
  {
    var wo = WorkOrder.Create(
                    id: Guid.NewGuid(),
                    vehicleId: Guid.NewGuid(),
                    startAt: DateTimeOffset.UtcNow,
                    endAt: DateTimeOffset.UtcNow.AddHours(1),
                    laborId: Guid.NewGuid(),
                    spot: Spot.A,
                    repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

    var result = wo.UpdateLabor(Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.LaborIdEmpty(wo.Id.ToString()).Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateSpot_ShouldReturnError_WhenSpotInvalid()
  {
    var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

    const Spot invalidSpot = (Spot)999;
    var result = wo.UpdateSpot(invalidSpot);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateTiming_ShouldReturnError_WhenInvalid()
  {
      var wo = WorkOrder.Create(
                        id: Guid.NewGuid(),
                        vehicleId: Guid.NewGuid(),
                        startAt: DateTimeOffset.UtcNow,
                        endAt: DateTimeOffset.UtcNow.AddHours(1),
                        laborId: Guid.NewGuid(),
                        spot: Spot.A,
                        repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

      var result = wo.UpdateTiming(DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow);

      Assert.False(result.IsSuccess);
      Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateState_ShouldReturnError_WhenTransitionInvalid()
  {
      var wo = WorkOrder.Create(
                    id: Guid.NewGuid(),
                    vehicleId: Guid.NewGuid(),
                    startAt: DateTimeOffset.UtcNow,
                    endAt: DateTimeOffset.UtcNow.AddHours(1),
                    laborId: Guid.NewGuid(),
                    spot: Spot.A,
                    repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

      var result = wo.UpdateState(State.Completed);

      Assert.False(result.IsSuccess);
      Assert.Equal(WorkOrderErrors.InvalidStateTransition(State.Scheduled, State.Completed).Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateLabor_ShouldReturnSuccess_WhenValidNewLaborId()
  {
    var wo = WorkOrder.Create(
                    id: Guid.NewGuid(),
                    vehicleId: Guid.NewGuid(),
                    startAt: DateTimeOffset.UtcNow,
                    endAt: DateTimeOffset.UtcNow.AddHours(1),
                    laborId: Guid.NewGuid(),
                    spot: Spot.A,
                    repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

    var newLaborId = Guid.NewGuid();
    var result = wo.UpdateLabor(newLaborId);

    Assert.True(result.IsSuccess);
    Assert.Equal(newLaborId, wo.LaborId);
  }

  [Fact]
  public void UpdateSpot_ShouldReturnSuccess_WhenSpotValid()
  {
    var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

    const Spot validSpot = Spot.B;
    var result = wo.UpdateSpot(validSpot);

    Assert.True(result.IsSuccess);
    Assert.Equal(Spot.B, wo.Spot);
  }

  [Fact]
  public void UpdateTiming_ShouldReturnSuccess_WhenValidTiming()
  {
      var wo = WorkOrder.Create(
                        id: Guid.NewGuid(),
                        vehicleId: Guid.NewGuid(),
                        startAt: DateTimeOffset.UtcNow,
                        endAt: DateTimeOffset.UtcNow.AddHours(1),
                        laborId: Guid.NewGuid(),
                        spot: Spot.A,
                        repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

      var result = wo.UpdateTiming(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));

      Assert.True(result.IsSuccess);
  }

  [Fact]
  public void UpdateState_ShouldReturnSuccess_WhenTransitionIsValid()
  {
      var wo = WorkOrder.Create(
                    id: Guid.NewGuid(),
                    vehicleId: Guid.NewGuid(),
                    startAt: DateTimeOffset.UtcNow,
                    endAt: DateTimeOffset.UtcNow.AddHours(1),
                    laborId: Guid.NewGuid(),
                    spot: Spot.A,
                    repairTasks: [RepairTasksFactory.CreateRepairTask().Value]).Value;

      var result = wo.UpdateState(State.InProgress);

      Assert.True(result.IsSuccess);
      Assert.Equal(State.InProgress, wo.State);
  }


}