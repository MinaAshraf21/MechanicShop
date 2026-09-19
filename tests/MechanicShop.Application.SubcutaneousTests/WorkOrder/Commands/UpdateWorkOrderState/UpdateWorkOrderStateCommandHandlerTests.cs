using System.Threading.Tasks;
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.UpdateWorkOrderState;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderStateCommandHandlerTests(WebAppFactory factory)
{
  private readonly IMediator _mediator = factory.CreateMediator();
  private readonly IAppDbContext _dbContext = factory.CreateDbContext();

  [Fact]
  public async Task Handle_WhenWorkOrderNotFound_ThenReturnFailure()
  {
    // Given
    var fakeWorkOrderId = Guid.NewGuid();
    // When
    var command = new UpdateWorkOrderStateCommand(fakeWorkOrderId, Domain.WorkOrders.Enums.State.InProgress);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenWorkOrderDidNotStart_ThenReturnFailure()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BVC 111").Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var fakeWorkOrderId = Guid.NewGuid();
    var startAt = DateTimeOffset.UtcNow.AddMinutes(30);
    var tasksDuration = (int)repairTask.EstimatedDuration;
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:fakeWorkOrderId,
                vehicleId: vehicle.Id,
                laborId: labor.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.SaveChangesAsync(default);
    // When
    var command = new UpdateWorkOrderStateCommand(fakeWorkOrderId, Domain.WorkOrders.Enums.State.InProgress);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc).Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenInvalidWorkOrderStateTransition_ThenReturnFailure()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "OKN 661").Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var fakeWorkOrderId = Guid.NewGuid();
    var startAt = DateTimeOffset.UtcNow.AddMinutes(-10);
    var tasksDuration = (int)repairTask.EstimatedDuration;
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:fakeWorkOrderId,
                vehicleId: vehicle.Id,
                laborId: labor.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.SaveChangesAsync(default);
    // When
    var newState = Domain.WorkOrders.Enums.State.Completed;
    var command = new UpdateWorkOrderStateCommand(fakeWorkOrderId, newState);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(WorkOrderErrors.InvalidStateTransition(workOrder.State, newState).Code, result.TopError.Code);
  }

}