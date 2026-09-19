using System.Threading.Tasks;
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.DeleteWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DeleteWorkOrderCommandHandlerTests(WebAppFactory factory)
{
  private readonly IMediator _mediator = factory.CreateMediator();
  private readonly IAppDbContext _dbContext = factory.CreateDbContext();

  [Fact]
  public async Task Handle_WhenWorkOrderNotFound_ThenReturnFailure()
  {
    // Given
    var fakeWorkOrderId = Guid.NewGuid();
    // When
    var command = new DeleteWorkOrderCommand(fakeWorkOrderId);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenWorkOrderStateIsNotScheduled_ThenReturnFailure()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "LLL 147").Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var fakeWorkOrderId = Guid.NewGuid();
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
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
    workOrder.UpdateState(Domain.WorkOrders.Enums.State.InProgress);

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.SaveChangesAsync(default);
    // When
    var command = new DeleteWorkOrderCommand(fakeWorkOrderId);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenWorkOrderStateIsScheduled_ThenReturnSuccess()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "ART 147").Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var fakeWorkOrderId = Guid.NewGuid();
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
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

    _dbContext.WorkOrders.Add(workOrder);
    await _dbContext.SaveChangesAsync(default);
    // When
    var command = new DeleteWorkOrderCommand(workOrder.Id);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsSuccess);
    Assert.Equal(fakeWorkOrderId, workOrder.Id);
  }
}